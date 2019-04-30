import json
import numpy as np
import os
import tensorflow as tf

from azureml.core.model import Model
from utils import FeatureExtractor
from accident import build_model

def init():
    global X, feat, sess, soft_pred, all_alphas, keep
    
    feat = FeatureExtractor(device='cpu')
    
    tf.reset_default_graph()
    X,keep,y,optimizer,loss,lstm_variables,soft_pred,all_alphas = build_model(batch_size=1)
    
    model_root = Model.get_model_path('crash-detection')
    saver = tf.train.import_meta_graph(os.path.join(model_root, 'model/final_model.meta'))
    
    sess = tf.Session()
    init = tf.global_variables_initializer()
    sess.run(init)
    saver.restore(sess, os.path.join(model_root, 'model/final_model'))

def run(raw_data):
    try:
        data = np.array(json.loads(raw_data)['data'])
        data = np.uint8(data)

        # get features
        features, boxes = feat.extract_features([data])
        features, boxes = features[0], np.int64(boxes[0])

        features = np.expand_dims(np.repeat(features[np.newaxis,...], 100, axis=0), axis=0)

        # make prediction
        [out, weights] = sess.run([soft_pred, all_alphas], feed_dict={X: features, keep: [0.0]})
        y_hat = out #np.argmax(out, axis=1)
        print("y_hat: ", y_hat)
        return y_hat.tolist(), weights.tolist(), boxes.tolist()
    except Exception as e:
    result = str(e)
    # return error message back to the client
    return json.dumps({"error": result})
