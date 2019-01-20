import serial
import time

print("dummy_response-start")


current_voltage = 12.0

with serial.Serial(port="COM27", baudrate=9600, bytesize=serial.SEVENBITS, parity=serial.PARITY_EVEN, stopbits=serial.STOPBITS_ONE) as ser:
    while(1):
        request = ser.readline().decode("cp932").strip()
        print("recv:"+request)
        if "VOLT?" in request:
            response = f"VOLT {current_voltage:.2f}\r"
            ser.write(response.encode("CP932"))
            print("send:"+response.strip())
        elif "VOLT " in request:# e.g. request => "VOLT 13.00"
            request = request.split(" ") # =>["VOLT", "13.00"]
            current_voltage = float(request[1]) #=> current_voltage=13.0

print("dummy_response-end")
