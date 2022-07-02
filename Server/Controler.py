import socket
from pynput import mouse, keyboard
# from pynput.mouse import Button, Controller
from win32api import GetSystemMetrics

class Handler:
    def __init__(self):
        self.m = mouse.Controller()
        self.k = keyboard.Controller()
        self.x_dim, self.y_dim = GetSystemMetrics(0),GetSystemMetrics(1)
        self.host = "127.0.0.1"
        self.port = 5901
        self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.s.bind((self.host, self.port))
        self.s.listen(1)
        self.option_type = -1
        self.option_args = []
        self.num = -1
        self.keyboard_laststate = 0
        self.key_map = [
            keyboard.KeyCode.from_char('`'),
            keyboard.KeyCode.from_char('1'),
            keyboard.KeyCode.from_char('2'),
            keyboard.KeyCode.from_char('3'),
            keyboard.KeyCode.from_char('4'),
            keyboard.KeyCode.from_char('5'),
            keyboard.KeyCode.from_char('6'),
            keyboard.KeyCode.from_char('7'),
            keyboard.KeyCode.from_char('8'),
            keyboard.KeyCode.from_char('9'),
            keyboard.KeyCode.from_char('0'),
            keyboard.KeyCode.from_char('-'),
            keyboard.KeyCode.from_char('='),
            keyboard.Key.backspace,
            keyboard.Key.tab,
            keyboard.KeyCode.from_char('q'),
            keyboard.KeyCode.from_char('w'),
            keyboard.KeyCode.from_char('e'),
            keyboard.KeyCode.from_char('r'),
            keyboard.KeyCode.from_char('t'),
            keyboard.KeyCode.from_char('y'),
            keyboard.KeyCode.from_char('u'),
            keyboard.KeyCode.from_char('i'),
            keyboard.KeyCode.from_char('o'),
            keyboard.KeyCode.from_char('p'),
            keyboard.KeyCode.from_char('['),
            keyboard.KeyCode.from_char(']'),
            keyboard.KeyCode.from_char('\\'),
            keyboard.Key.caps_lock,
            keyboard.KeyCode.from_char('a'),
            keyboard.KeyCode.from_char('s'),
            keyboard.KeyCode.from_char('d'),
            keyboard.KeyCode.from_char('f'),
            keyboard.KeyCode.from_char('g'),
            keyboard.KeyCode.from_char('h'),
            keyboard.KeyCode.from_char('j'),
            keyboard.KeyCode.from_char('k'),
            keyboard.KeyCode.from_char('l'),
            keyboard.KeyCode.from_char(';'),
            keyboard.KeyCode.from_char('\''),
            keyboard.Key.enter,
            keyboard.Key.shift_l,
            keyboard.KeyCode.from_char('z'),
            keyboard.KeyCode.from_char('x'),
            keyboard.KeyCode.from_char('c'),
            keyboard.KeyCode.from_char('v'),
            keyboard.KeyCode.from_char('b'),
            keyboard.KeyCode.from_char('n'),
            keyboard.KeyCode.from_char('m'),
            keyboard.KeyCode.from_char(','),
            keyboard.KeyCode.from_char('.'),
            keyboard.KeyCode.from_char('/'),
            keyboard.Key.shift_r,
            keyboard.Key.ctrl_l,
            keyboard.Key.alt_l,
            keyboard.KeyCode.from_char(' '),
            keyboard.Key.alt_r,
            keyboard.Key.ctrl_r,
            keyboard.Key.up,
            keyboard.Key.down,
            keyboard.Key.left,
            keyboard.Key.right
        ]
    
    def Exe(self, type, args = []):
    #1 x y代表drag(x,y)，这里x和y是0到1间的数,2代表左键按下, 
    #3代表左键松开,4代表右键按下,5代表右键松开,6 state 表示 state 是键盘状态
        print("exe", type, args)
        cur_x, cur_y = self.m.position
        if type == 1:
            x, y = args
            x = self.x_dim * x // 1000
            y = self.y_dim * y // 1000
            self.m.position=(x, y)
        elif type == 2:
            self.m.press(mouse.Button.left)
        elif type == 3:
            self.m.release(mouse.Button.left)
        elif type == 4:
            self.m.press(mouse.Button.right)
        elif type == 5:
            self.m.release(mouse.Button.right)
        elif type == 6:
            keyboard_curstate = args[0]
            for i in range(0, 62):
                if self.keyboard_laststate & ~keyboard_curstate & 1 << i:
                    ch = self.key_map[i]
                    print(ch)
                    self.k.release(ch)
                elif ~self.keyboard_laststate & keyboard_curstate & 1 << i:
                    ch = self.key_map[i]
                    print(ch)
                    self.k.press(ch)
            self.keyboard_laststate = keyboard_curstate

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
