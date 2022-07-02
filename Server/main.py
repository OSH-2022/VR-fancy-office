import socket
from pynput import mouse, keyboard
from win32api import GetSystemMetrics
import ctypes as ct

class cUnion(ct.Union):
    _fields_ = [('ull', ct.c_ulonglong), ('d', ct.c_double)]

KEYMAP = [
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

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('', 5901))
server_socket.listen(1)
print('Server started successfully')
while True:
    print('Waiting for connection...')
    client_socket, addr = server_socket.accept()
    print('Accepted connection from', addr)
    i = 0
    qword = 0
    laststate = 0
    x = cUnion()
    y = cUnion()
    while True:
        try:
            data = client_socket.recv(1024)
            if len(data) == 0:
                break
        except:
            break
        else:
            upos = 0
            for ch in data:
                qword = qword << 8 | (ch & 255)
                i += 1
                if i == 1:
                    qword = 0
                    upos = ch
                if i == 9:
                    x.ull = qword
                    qword = 0
                elif i == 17:
                    y.ull = qword
                    qword = 0
                    if upos:
                        mouse.Controller().position = (int(x.d * GetSystemMetrics(0) + 0.5), int(y.d * GetSystemMetrics(1) + 0.5))
                elif i == 25:
                    for j in range(0, 62):
                        if laststate & ~qword & 1 << j:
                            print('Released key #{}'.format(j))
                            keyboard.Controller().release(KEYMAP[j])
                        elif ~laststate & qword & 1 << j:
                            print('Pressed key #{}'.format(j))
                            keyboard.Controller().press(KEYMAP[j])
                    if (laststate ^ qword) & 0x3fffffffffffffff:
                        print(qword & 0x3fffffffffffffff)
                    if laststate & ~qword & 0x4000000000000000:
                        print('Released left button')
                        mouse.Controller().release(mouse.Button.left)
                    elif ~laststate & qword & 0x4000000000000000:
                        print('Pressed left button')
                        mouse.Controller().press(mouse.Button.left)
                    if laststate & ~qword & 0x8000000000000000:
                        print('Released right button')
                        mouse.Controller().release(mouse.Button.right)
                    elif ~laststate & qword & 0x8000000000000000:
                        print('Pressed right button')
                        mouse.Controller().press(mouse.Button.right)
                    i = 0
                    laststate = qword
                    qword = 0
    client_socket.close()
    print('Connection closed')
