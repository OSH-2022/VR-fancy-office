import socket
import datetime
import math
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect(('127.0.0.1', 6666))
roll=0
pitch=0
yaw=0
velocity=200000
height=4000000
LastUpdateTime = datetime.datetime.now()
while True:
    data=s.recv(1024).decode()
    print('received',data)
    param=data.split()
    AileronLeftRot=int(param[0])
    ElevatorsRot=int(param[2])
    Accelerate=int(param[3])
    RubberRot=int(param[4]);
    AileronLeftRot=float(AileronLeftRot)/1000
    ElevatorsRot=float(ElevatorsRot)/1000
    Accelerate=float(Accelerate)/1000
    RubberRot=float(RubberRot)/1000
    timedelta=(datetime.datetime.now()-LastUpdateTime).microseconds
    LastUpdateTime=datetime.datetime.now()
    roll+=AileronLeftRot*timedelta/1000
    pitch+=ElevatorsRot*timedelta/1000
    yaw+=RubberRot*timedelta/1000
    velocity+=Accelerate*timedelta/1000
    height+=velocity*math.sin(pitch/180*math.pi)
    msg=(str(int(roll))+" "+str(int(pitch))+" "+str(int(yaw))+" "+str(int(velocity))+" "+str(int(height))).encode()
    s.send(msg)
