import socket
from cryptography.fernet import Fernet

# Create Socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(('localhost', 12345))

# Generate Key
key = Fernet.generate_key()
cipher_suite = Fernet(key)

# Loading Socket
sock.listen(1)
client_socket, client_address = sock.accept()

# Client Data
data = client_socket.recv(1024)

# Crypted Data
encrypted_data = cipher_suite.encrypt(data)

# Отправка зашифрованных данных через прокси-сервер
proxy_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
proxy_socket.connect(('proxy_address', 8080))
proxy_socket.send(encrypted_data)

# Получение ответа от прокси-сервера
proxy_data = proxy_socket.recv(1024)

# Расшифровка ответа
decrypted_data = cipher_suite.decrypt(proxy_data)

# Отправка расшифрованного ответа клиенту
client_socket.send(decrypted_data)

# Закрытие соединений
client_socket.close()
proxy_socket.close()
