# RemoteProcessManager

### First build RemoteProcessManager and WorkerTester projects

### For Redis run docker-compose.yaml

## To start a Agent open terminal at RemoteProcessManager bin directory

```ps
    .\RemoteProcessManager.exe 1 video1 127.0.0.1:6379 5001
```

* 1 - RemoteProcessManager Mode - 1 => Agent, 2 => AgentProxy
* video1 - AgentName, building message broker topics from this name
* 127.0.0.1:6379 - Message broker url
* 5001 - RemoteProcessManager http Port

## To start a AgentProxy open terminal at RemoteProcessManager bin directory

```ps
    .\RemoteProcessManager.exe 2 video1 127.0.0.1:6379 5002 "C:\dev\RemoteProcessManager\WorkerTester\bin\Debug\net6.0\WorkerTester.exe" "a1 a2 a3"
```

* 2 - RemoteProcessManager Mode - 1 => Agent, 2 => AgentProxy
* video1 - AgentName, building message broker topics from this name
* 127.0.0.1:6379 - Message broker url
* 5002 - RemoteProcessManager http Port
* "C:\dev\RemoteProcessManager\WorkerTester\bin\Debug\net6.0\WorkerTester.exe" - ProcessFullName to send Agent
* "a1 a2 a3" - ProcessArguments to send Agent
