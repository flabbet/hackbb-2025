import mss

class ScreenCapture:
    def __init__(self, monitor_index=1):
        self.sct = mss.mss()
        self.monitor = self.sct.monitors[monitor_index]

    def read(self):
        img = np.array(self.sct.grab(self.monitor))
        frame = cv2.cvtColor(img, cv2.COLOR_BGRA2BGR)
        return True, frame 
        
    def release(self):
        self.sct.close()
