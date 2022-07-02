import socket
import threading
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
s.bind(('127.0.0.1', 6666))
s.listen(10)
conn1,_ = s.accept()
conn2,_ = s.accept()
def translate(conn1,conn2):
    while True:
        try:
            data=conn1.recv(1024)
            print('received',data.decode(),'from conn1')
            conn2.send(data)
        except:
            conn2.close()
            exit()
        
threading.Thread(target=translate,args=(conn1,conn2)).start()
threading.Thread(target=translate,args=(conn2,conn1)).start()
