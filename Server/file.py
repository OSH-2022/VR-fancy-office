import socket
import os

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
    splitFileName = os.path.split(fileName).encode('utf-8')
    client_socket.send(bytes([ splitFileName.length() ]))
    client_socket.send(splitFileName)
    if os.path.exists(fileName):
        with open(fileName, 'rb') as f:
            context = read()
            client_socket.send(bytes([ len(context) >> 24, len(context) >> 16 & 255, len(context) >> 8 & 255, len(context) & 255 ]))
            client_socket.send(context)
    else:
        client_socket.send(bytes([ 0, 0, 0, 0 ]))
    client_socket.close()
    print('Connection closed')
