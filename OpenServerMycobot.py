from pymycobot.mycobot import MyCobot
import time
import socket

SERVER_IP = '172.16.2.175' #mycobotIP
SERVER_PORT = 11001

server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_socket.bind((SERVER_IP, SERVER_PORT))

print("Server connect...")

mc = MyCobot('/dev/ttyAMA0',1000000)
mc.send_angles([0,0,0,0,-90,0],50)
time.sleep(2)

print("MyCobot Ready...")

while True:

    data, address = server_socket.recvfrom(1024)
    
    print(f"Receive messages from clients: {data.decode()}")
    str = data.decode()
    str_list = str.split('/')
    str_list = [int(float(item)) for item in str_list]
    J1,J2,J3,J4,J5,J6,speed = str_list
    #print("x ::: ", type(x))
    mc.set_color(255,0,0)
    mc.send_angles([J1,J2,J3,J4,J5,J6],speed)
    #mc.send_coords([x,-110,410,-90,0,170],50,0)
    print(J1,J2,J3,J4,J5,J6,speed)
    time.sleep(0.01)
    mc.set_color(0,255,0)
    


