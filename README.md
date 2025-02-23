## Skynet斗地主游戏demo 使用说明

### 服务端启动配置

1.创建一个本地的数据库 在Env 节点中配置db标签中的数据库地址和数据库账号密码，新建一张account的用户表包含用户id 和密码。

2.Env中配置lua引用模块路径，main标签配置启动项lua脚本，网关服务中配置服务器地址和监听端口。

3.gitbash下命令进入到服务器根目录启动 ./skynet ./env

### 客户端工程配置

1.配置WebSocket 连接地址。

  2.unity发布PC 即可。
