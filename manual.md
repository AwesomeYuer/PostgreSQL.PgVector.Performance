# 服务器虚机
```sh
# 免密ssh远程登录服务器

# 客户端生成

ssh-keygen -t rsa

# 客户端打开生成 Windows: C:\Users\<userName>\.ssh\id_rsa.pub 文件
# 编辑如下命令行在远程 linux 服务端执行后即可免密远程 ssh

echo ssh-rsa ZZZZB3NzaC1yc2EAAAADAQABAAABgQCoOqAxklUB39B6QbT4zIgTc+bAf2CrICZ/saL7l9uiUsWMLom0D1i7yrO5AE3j5Hz24baYLGUTBbr8PwlJNxj9Dy7bdhYPYifqpUC0XOCwgCuOOb9jCwLljV2NFQAZ5Chp9BXC8AllJGoWwsdpBTyQeNQWVQSwo+bjyCEHb/l16TdVj1qdI6NxgkPHfCwndg71bDH/Fh4bFZj5lrQ2VWSSsJ0SV7RF0Ye0zF2B0LScvb+WEDTAoAh/qhDEEbelkVSFkF3mm1rR8aXWCZJeUcxdA5WQ+aetI4YL2g6DO7V25bB8Wrk0hn0qGjURxFFly8m0OFMhNKOnDfCBwZVYKMQWbw8NqdpLa2CJ5En8uBSD2qwieM5MnEe97ftZCnDNd2Aky+D+TaML4+aw4bHa+LCniILj9IJSy7Nd6IGy0ATusRW71nbST1f6N1DukuAq/kYHwLJHx+yzaijDdMlKvhI8y8FSt19+BgU5R4Ns1TbJzWBkaTuWQte6D7Tj+00ZZZZ= AwesomeYuer>>~/.ssh/authorized_keys

# https://learn.microsoft.com/en-us/azure/virtual-machines/linux/attach-disk-portal?tabs=ubuntu

# Find the disk 查找加载附加磁盘
# Once connected to your VM, you need to find the disk. In this example, we're using lsblk to list the disks.
# 连接到 VM 后，您需要找到磁盘。在此示例中，我们使用 lsblk 列出磁盘。

lsblk -o NAME,HCTL,SIZE,MOUNTPOINT | grep -i "sd"

# Prepare a new empty disk 准备新的空盘
# The following example uses parted on /dev/sdc, which is where the first data disk will typically be on most VMs. Replace sdc with the correct option for your disk. We're also formatting it using the XFS filesystem.
# The following example uses parted on /dev/sdc, which is where the first data disk will typically be on most VMs. Replace sdc with the correct option for your disk. We're also formatting it using the XFS filesystem.
# 以下示例在 /dev/sdc 上使用 parted ，这是第一个数据磁盘通常位于大多数 VM 上的位置。将 sdc 替换为适合您的磁盘的正确选项。我们还使用 XFS 文件系统对其进行格式化。Prepare a new empty disk 准备新的空盘

sudo parted /dev/sdc --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/sdc1
sudo partprobe /dev/sdc1

sudo fdisk -l

sudo apt-get install nfs-common

sudo mkfs -t ext4 /dev/sdc1

# Mount the disk 挂载磁盘
# Create a directory to mount the file system using mkdir. The following example creates a directory at /datadrive:
# 使用 mkdir 创建目录以挂载文件系统。以下示例在 /userdata、~/data 处创建一个目录

sudo mkdir /userdata
sudo mkdir ~/data

# Use mount to then mount the filesystem. The following example mounts the /dev/sdc1 partition to the /datadrive mount point:
# 然后使用 mount 挂载文件系统。以下示例将 /dev/sdc1 分区挂载到 /datadrive 挂载点：

# sudo mount /dev/sdc1 /userdata
# sudo mount /dev/sdc1 ~/data

sudo chmod 777 ~/data
sudo chmod 777 /userdata


# 上命令挂载的磁盘只会在当前运行系统过程中有效，服务器重启则会丢掉挂载。
# 重启自动挂载
# 找到挂载盘的 UUID
# 该命令会列出所有磁盘的信息，如 /dev/sda , /dev/sdb的 uuid 。可以配合使用grep 命令查找对应磁盘。

ls -l /dev/disk/by-uuid

# 备份 fstab 文件

cp /etc/fstab /etc/fstab.bak

# 修改 fstab 文件
# https://blog.csdn.net/wohu1104/article/details/121021207

sudo nano /etc/fstab

# add rows as below:
# UUID=da83d5a3-bb4c-473d-be1a-cec2cd63fae2 /userdata        ext4    defaults 0 2
# UUID=da83d5a3-bb4c-473d-be1a-cec2cd63fae2 /home/<userName>/data        ext4    defaults 0 2

```

# Docker
```sh

sudo apt install docker.io

sudo docker ps -a

sudo apt install docker-compose

docker system df

# 一键删除所有已经停止的容器
# https://zhuanlan.zhihu.com/p/100793598
# https://zhuanlan.zhihu.com/p/31820191
# docker container prune

```

# Redis RediSearch Docker
```
cd openai-cookbook-python/examples/vector_databases/redis/

sudo docker ps -a

sudo docker-compose up -d

sudo docker ps -a

telnet localhost 6379

sudo apt install redis-tools

redis-cli

# in redis-cli 设密码, 重启即失效, 必须重设
config set requirepass password01!

auth password01!

config get requirepass

quit

```

# Python3 环境
```
python3 --version

sudo apt install python3-pip

sudo python3 -m pip install ipykernel -U --user --force-reinstall

pip install numpy

pip install pandas

pip install nbutils

pip install wget

# 保存在 openai-cookbook-python/examples/vector_databases/redis/

wget https://cdn.openai.com/API/examples/data/vector_database_wikipedia_articles_embedded.zip

# vscode 打开 Dev 分支运行:
# openai-cookbook-python/examples/vector_databases/redis/RediSearchVectorQueries.ipynb

```

# PostgresSQL + PgVector + Docker
# ssh
```sh

docker pull ankane/pgvector

docker run --name test-pgvector -v ~/temp:/share -e POSTGRES_PASSWORD=password01! -d -p 5432:5432 ankane/pgvector

```

# 压测控制台
```

sudo docker pull ikende/beetlex_webapi_benchmark:v0.8.6

sudo docker run -d -p 9090:9090 ikende/beetlex_webapi_benchmark:v0.8.6

```


# 测试过程中的性能监控
```sh
# 本地监控
sudo docker stats

# 远程监控


```