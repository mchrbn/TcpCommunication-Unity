# TcpCommunication - Unity

A simple way to communicate between a Unity client and a server via TCP.

## Use

1. Create an empty GameObject and add the TcpCommunication script on it.
2. Set your server IP and port.
3. You can send bytes to your server by using :

```
bool hasSent = TcpCommunication.Instance.SendBytes(yourBytesArray);
```

4. When the server sent you messages, you have to use a delegate in the class/object you need to receive that message:

```
void OnEnable(){
    TcpCommunication.Instance.OnReceivedMessage += OnReceivedMessage;
}

void OnDisable(){
    TcpCommunication.Instance.OnReceivedMessage -= OnReceivedMessage;
}

void OnReceivedMessage(string str){
    //Do something with your string.
}
```