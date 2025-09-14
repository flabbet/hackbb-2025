import math

class FaceTracker:
    def __init__(self, max_distance=200, max_missing=10):
        self.next_id = 0
        self.tracks = {}  # {id: {"bbox": (x,y,w,h), "missed": 0}}
        self.max_distance = max_distance
        self.max_missing = max_missing

    def _center(self, bbox):
        x, y, w, h = bbox
        return (x + w/2, y + h/2)

    def update(self, detections):
        updated_tracks = {}
        used = set()

        # Match detections to existing tracks
        for tid, data in self.tracks.items():
            best_match = None
            best_dist = float("inf")
            cx1, cy1 = self._center(data["bbox"])

            for i, det in enumerate(detections):
                if i in used:
                    continue
                cx2, cy2 = self._center(det)
                dist = math.hypot(cx1 - cx2, cy1 - cy2)
                if dist < self.max_distance and dist < best_dist:
                    best_match = i
                    best_dist = dist

            if best_match is not None:
                updated_tracks[tid] = {"bbox": detections[best_match], "missed": 0}
                used.add(best_match)
            else:
                # keep old track but increment missed counter
                if data["missed"] < self.max_missing:
                    updated_tracks[tid] = {"bbox": data["bbox"], "missed": data["missed"] + 1}

        # Add new detections that didn’t match any track
        for i, det in enumerate(detections):
            if i not in used:
                updated_tracks[self.next_id] = {"bbox": det, "missed": 0}
                self.next_id += 1

        self.tracks = updated_tracks
        return self.tracks

    def getByDetection(self, detection):
        for tid, data in self.tracks.items():
            best_match = None
            best_dist = float("inf")
            cx1, cy1 = self._center(data["bbox"])
            cx2, cy2 = self._center(detection)

            if (cx1 == cx2 and cy1 == cy2):
                return tid
        return -1
