### MicroCMIP集群部署

MicroCMIP支持分布式服务，下面演示如何部署一个中间件集群。

#### 分布式中间件原理

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/6BE5331C2F3D4504BC4DC9FE1AA6EE74/19937)

如上图所示树形中间件集群，节点4和节点5都装载了Books.Service服务，客户端连接节点3，而节点3并没有装载本地Books.Service服务，只是配置了远程Books.Service服务并指向的执行节点包括节点4和节点5，所以客户端会远程调用节点4或节点5上的Books.Service服务，到底调用哪个节点？中间件提供负载均衡的两种算法，最短路径与随机路径，最后计算节点路径：**efwplus://节点3/节点1/根节点/节点2/节点4**，然后中间件会按照此路径进行远程调用服务。

#### 新增中间件节点

在中心平台配置的完整流程：
>1. 注册中间件节点，并获取注册码
>2. 注册插件服务
>3. 给新增的节点配置相关的本地服务和远程服务
>4. 修改中间件节点连接上级节点的通讯地址，并启动新增的中间件
>5. 在平台上可以查看节点的监控图

* 注册中间件节点

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/07279F6187944118A5B515AF746A3F5E/19880)

* 注册插件服务

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/5D274E3835414B65B453C39B6BFA7FEC/19888)

* 配置节点服务
 
![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/23A753427C6B46C88D3FEEF83440146C/19890)

* 启动新增节点

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/67B415284AD44FD184B0C02C1C0F30B0/19892)

* 节点监控图

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/37E0059EED3C45648CAB9232F5CFA29C/19894)

##### 测试调用分布式服务

> 首先根节点和节点1都配置了Books.Service为本地服务，那么测试调用根节点的服务，根节点将直接返回数据，节点1不会收到请求。

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/72C1A9D6908147F79E6B373F0C763AE9/19929)

> 然后根节点把Books.Service配置为远程服务,节点1配置Books.Service为本地服务，那么测试调用根节点的服务，请求会转发到节点1，返回数据回跟节点输出。

![image](http://note.youdao.com/yws/public/resource/e569ed963bafc504c900f522d7a9cdec/xmlnote/D2EB9A0F9B1245468D51C3E9D549F9B1/19931)