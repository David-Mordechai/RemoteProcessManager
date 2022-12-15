# RemoteProcessManager

## First build RemoteProcessManager and WorkerTester projects

## For Redis run docker-compose.yaml

## To start a Agent open terminal at RemoteProcessManager bin directory

```ps
    .\RemoteProcessManager.exe 1 5001 127.0.0.1:6379 process-video1 stream-video1</code>
```

* 1 - RemoteProcessManager Mode - 1 => Agent, 2 => AgentProxy
* 5001 - RemoteProcessManager http Port
* 127.0.0.1:6379 - Message broker url
* process-video1 - Topic for sending process fullname to create
* stream-video1 - Topic to stream logs back to AgentProxy

## To start a AgentProxy open terminal at RemoteProcessManager bin directory

```ps
    .\RemoteProcessManager.exe 2 5002 127.0.0.1:6379 process-video1 stream-video1 C:\dev\RemoteProcessManager\WorkerTester\bin\Debug\net6.0\WorkerTester.exe
```

* 2 - RemoteProcessManager Mode - 1 => Agent, 2 => AgentProxy
* 5002 - RemoteProcessManager http Port
* 127.0.0.1:6379 - Message broker url
* process-video1 - Topic for publish process fullname to Agent
* stream-video1 - Topic to subscribe for logs stream from Agent
* C:\dev\RemoteProcessManager\WorkerTester\bin\Debug\net6.0\WorkerTester.exe - ProcessFullName to send Agent
