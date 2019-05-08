import json
import numpy as np
import os
import tensorflow as tf
import base64

from PIL import Image
from io import BytesIO

from azureml.core.model import Model
from utils import FeatureExtractor
from accident import build_model

def init():
    global X, feat, sess, soft_pred, all_alphas, keep

    feat = FeatureExtractor(device='cpu')

    tf.reset_default_graph()
    X,keep,y,optimizer,loss,lstm_variables,soft_pred,all_alphas = build_model(batch_size=1)

    model_root = Model.get_model_path('crash-detection')
    saver = tf.train.Saver()

    config=tf.ConfigProto(allow_soft_placement=True)
    config.gpu_options.allow_growth=True
    sess = tf.Session(config=config)
                          
    init = tf.global_variables_initializer()
    sess.run(init)
    saver.restore(sess, os.path.join(model_root, 'model/final_model'))

def load(data):
    data = base64.b64decode(data)
    pil_image = Image.open(BytesIO(data)).convert("RGB")
    # convert to BGR format
    image = np.array(pil_image)[:, :, [2, 1, 0]]
    return image    

def run(raw_data):
    try:
        data = json.loads(raw_data)['data']
        data = load(data)

        # get features
        features, boxes = feat.extract_features([data])
        features, boxes = features[0], np.int64(boxes[0])

#         features = np.expand_dims(np.repeat(features[np.newaxis,...], 100, axis=0), axis=0)
        features = np.expand_dims(np.vstack((features[np.newaxis,...], np.repeat(np.zeros([1,20,1024]), 99, axis=0))), axis=0)

        # make prediction
        [out, weights] = sess.run([soft_pred, all_alphas], feed_dict={X: features, keep: [0.0]})
        y_hat = out #np.argmax(out, axis=1)
#         print("y_hat: ", y_hat)
        return y_hat.tolist(), weights.tolist(), boxes.tolist()

    except Exception as e:
        result = str(e)
        print(result)
        # return error message back to the client
        return {"error": result, "data":raw_data}
