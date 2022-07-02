import socket
import os

def sendfile(client_socket, fileName):
    splitFileName = os.path.split(fileName)[1].encode('utf-8')
    if os.path.isdir(fileName):
        client_socket.send(bytes([ 1, len(splitFileName) ]))
        client_socket.send(splitFileName)
        dirs = os.listdir(fileName)
        for file in dirs:
            sendfile(client_socket, os.path.join(fileName, file))
        client_socket.send(bytes([ 3 ]))
    else:
        with open(fileName, 'rb') as f:
            client_socket.send(bytes([ 2, len(splitFileName) ]))
            client_socket.send(splitFileName)
            context = f.read()
            client_socket.send(bytes([ len(context) >> 24, len(context) >> 16 & 255, len(context) >> 8 & 255, len(context) & 255 ]))
            client_socket.send(context)

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('', 5902))
server_socket.listen(1)
print('Server started successfully')

while True:
    print('Waiting for connection...')
    client_socket, addr = server_socket.accept()
    print('Accepted connection from', addr)
    fileNameLen = client_socket.recv(4)
    fileNameLen = (int(fileNameLen[0]) & 255) << 24 | (int(fileNameLen[1]) & 255) << 16 | (int(fileNameLen[2]) & 255) << 8 | (int(fileNameLen[3]) & 255)
    fileName = client_socket.recv(fileNameLen).decode('utf-8')
    if os.path.exists(fileName):
        sendfile(client_socket, fileName)
    client_socket.send(bytes([ 3 ]))
    client_socket.close()
    print('Connection closed')
