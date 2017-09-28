using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class TcpCommunication : MonoBehaviour {

	private static TcpCommunication _instance;
	public static TcpCommunication Instance {get {return _instance; }}

	public bool isConnected = false;

	Thread tcpThread;
	bool isThreadRunning = false;

	TcpClient client;
	public string ip = "127.0.0.1";
	public int port = 5000;

	byte[] bytesToSend = null;

	Queue<string> msgQueue = new Queue<string>();

	
	public delegate void ReceivedMessage(string str);
	public event ReceivedMessage OnReceivedMessage;


	void Awake()
	{
		if(_instance != null && _instance != this){
			Destroy(this.gameObject);
		}
		else{
			_instance = this;
			isThreadRunning = true;
			client = new TcpClient();
			tcpThread = new Thread(new ThreadStart(TCPThread));
			tcpThread.Start();
		}
	}

	void Update()
	{
		lock(msgQueue){
			while(msgQueue.Count > 0){
				string str = msgQueue.Dequeue();
				if(OnReceivedMessage != null)
					OnReceivedMessage(str);
			}
		}
	}

	public bool SendBytes(byte[] bytes){

		if(!isConnected) return false;

		//Skip if didn't send previous bytes
		if(bytesToSend != null){
			Debug.Log("Warning : Previous bytes not finished sending...");
			return false;
		}

		bytesToSend = bytes;
		return true;
	}

	public void SendTestBytes(){
		float[] test = {3.133f, 4.313f, 5.193f, 1000.2939f, 200.03f};
		bytesToSend = Utilities.FloatsToBytes(test);
	}

	void OnApplicationQuit()
	{
		//Close client and abort thread
		isThreadRunning = false;
		client.Close();
		tcpThread.Abort();
	}

	void TCPThread(){
		while(isThreadRunning){
			//If connected handle write / read sockets
			if(client.Client.Connected){
				NetworkStream stream = client.GetStream();

				//Send stream to server, if there is something
				if(bytesToSend != null && stream.CanWrite){
					Debug.Log("Send bytes with length: " + bytesToSend.Length);
					stream.Write(bytesToSend, 0, bytesToSend.Length);
					bytesToSend = null;
				}

				//Read the stream if any received by the server
				if(stream.CanRead){
					byte[] buffer = new byte[client.ReceiveBufferSize];
					int nbrBytes = 0;
					string str = "";
					while (stream.DataAvailable) {
						nbrBytes += stream.Read(buffer, 0, (int) client.ReceiveBufferSize);
						str += Encoding.UTF8.GetString(buffer);
					}

					if(nbrBytes > 0){
						if(str.Length > 0){
							lock(msgQueue){
								msgQueue.Enqueue(str);
							}
						}
					}
				}
			}

			//If not connected
			else{
				Debug.Log("Connecting to TCP Server");
				try{
					client.Connect(ip, port);
					isConnected = true;
					Debug.Log("Connected to TCP Server");
				}
				catch(Exception e){
					isConnected = false;
					Debug.Log("Connection to TCP Server failed : " + e.ToString());
					Thread.Sleep(2000);
				}
			}
		}

		Debug.Log("Thread stopped.");
		if(client != null) client.Close();
	}
}
