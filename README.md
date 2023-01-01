# RemoteProcessManager

### First build RemoteProcessManager and WorkerTester projects

### For Redis run docker-compose.yaml

### To start a Agent or AgentProxy open terminal at RemoteProcessManager bin directory
### Arguments

![image](https://user-images.githubusercontent.com/53663592/209629986-f869533a-ad1b-4076-95a7-f1afee47d2a7.png)

Agent example
```ps
.\RemoteProcessManager.exe --agent-mode 1 --agent-name video1 --messageBroker-url 127.0.0.1:6379 --http-port 5001
```

AgentProxy example
```ps
.\RemoteProcessManager.exe --agent-mode 2 --agent-name video1 --messageBroker-url 127.0.0.1:6379 --http-port 5002 --process-name "C:\dev\RemoteProcessManager\WorkerTester\bin\Debug\net6.0\WorkerTester.exe" --process-args "\"-n david -a a1\""
```
## Remote Process Arguments - Important!!!
### If you want to pass remote process arguments then you should passed within a string, 
### example: "\"-n david -a a1\""
