import socket
from pynput.mouse import Button, Controller
from win32api import GetSystemMetrics

class Handler:
    def __init__(self):
        self.m = Controller()
        #self.k = PyKeyboard()
        self.x_dim, self.y_dim = GetSystemMetrics(0),GetSystemMetrics(1)
        self.host = "127.0.0.1"
        self.port = 5901
        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s.bind((self.host, self.port))
        self.s.listen(1)
        self.option_type = -1
        self.option_args = []
        self.num = -1
    
    def Exe(self, type, args = []):
    #1 x y代表drag(x,y)，这里x和y是0到1间的数,2代表左键按下, 
    #3代表左键松开,4代表右键按下,5代表右键松开,6 ch表示某个键盘的键按下,7 ch表示松开
        print("exe", type, args)
        cur_x, cur_y = self.m.position
        if type == 1:
            x, y = args
            x = self.x_dim * x // 1000
            y = self.y_dim * y // 1000
            self.m.position=(x, y)
        elif type == 2:
            self.m.press(Button.left)
        elif type == 3:
            self.m.release(Button.left)
        elif type == 4:
            self.m.press(Button.right)
        elif type == 5:
            self.m.release(Button.right)
            print(ch)
            #self.k.press_key(chr(ch))
        elif type == 7:
            ch = args[0]
            print(ch)
            #self.k.release_key(chr(ch))

    def GetInt(self, x):
        print("GETEINT", x)
        if self.option_type == -1:
            if x != 1 and x != 6 and x != 7 :
                self.Exe(x)
            else: 
                self.option_type = x
        else:
            self.option_args.append(x)
            if self.option_type == 1 :
                if len(self.option_args) == 2:
                    self.Exe(1, self.option_args)
                    self.option_type = -1
                    self.option_args = []
            elif self.option_type == 6 or self.option_type == 7 :
                if len(self.option_args) == 1:
                    self.Exe(self.option_type, self.option_args)
                    self.option_type = -1
                    self.option_args = []

    def GetChar(self, ch):
        if ch < '0' or ch > '9':
            if self.num != -1:
                self.GetInt(self.num)
                self.num = -1
        else :
            self.num = int(ch) if self.num == -1 else self.num * 10 + int(ch)

    def Connect(self):
        self.client, _ = self.s.accept()
        print('Connected!')
    
    def Run(self):
        while True :
            data = self.client.recv(1024).decode("-utf8")
            if len(data) <= 0:
                break
            for x in data:
                self.GetChar(x)
            print(data)
h = Handler()
while True:
    h.Connect()
    h.Run()
