from maskrcnn_benchmark.config import cfg
from predictor import COCODemo
import numpy as np

class FeatureExtractor():

    def __init__(self):
      self.config_file = "../maskrcnn-benchmark/configs/caffe2/e2e_faster_rcnn_R_101_FPN_1x_caffe2.yaml"
      self.cfg = cfg
      cfg.merge_from_file(self.config_file)
      cfg.merge_from_list(["MODEL.DEVICE", "cuda"])
      cfg.merge_from_list(["MODEL.MASK_ON", False])

      self.coco_demo = COCODemo(
      cfg,
      min_image_size=800,
      confidence_threshold=0.5,
      )

    def extract_features(self, batch_images, k=20):
      box_list = self.coco_demo.extract_encoding_features(batch_images)
      features = []
      for boxes in box_list:
        # Only keep top k predictions
        boxes = boxes[:k]
        encodings = boxes.get_field("encoding_features")
        # Pad features with 0 to make them size k
        encodings = np.pad(encodings, [(0, k-len(encodings)), (0, 0)], mode='constant')
        features.append(encodings)
      return np.array(features)
