from sys import flags, argv
from scapy.all import *
from scapy.layers.inet import TCP, IP


def tcp_connect(target, port_range):
    for port in port_range:
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            address = (target, port)
            sock.connect(address)
            print("Port " + str(port) + " is open")
            sock.close()
        except:
            pass


def port_scan(target, ports, scan_type):
    print(target, ports)
    if "-" in ports:
        ports = ports.split("-")
        port_range = range(int(ports[0]), int(ports[1]))
    else:
        port_range = [int(ports)]

    if scan_type == "half":
        flags = "S"
    elif scan_type == "null":
        flags = ""
    elif scan_type == "fin":
        flags = "F"
    elif scan_type == "xmas":
        flags = "FPU"
    elif scan_type == "connect":
        tcp_connect(target, port_range)
        return

    for port in port_range:
        package = IP(dst=target) / TCP(dport=port, flags=flags)
        response = sr1(package, timeout=0.1, verbose=0)
        if response is None:
            if scan_type != "half":
                print("port " + str(port) + " is open")
        else:
            if scan_type == "half" and response[1].flags == "SA":
                print("port " + str(port) + " is open")


if __name__ == "__main__":
    if len(sys.argv) == 4:
        port_scan(target=argv[1], ports=argv[2], scan_type=argv[3])
