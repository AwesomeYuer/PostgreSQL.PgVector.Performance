# 服务器虚机
```sh
# 免密ssh远程登录服务器

# 客户端生成

ssh-keygen -t rsa

# 客户端打开生成 Windows: C:\Users\<userName>\.ssh\id_rsa.pub 文件
# 编辑如下命令行在远程 linux 服务端执行后即可免密远程 ssh

echo ssh-rsa ZZZZB3NzaC1yc2EAAAADAQABAAABgQCoOqAxklUB39B6QbT4zIgTc+bAf2CrICZ/saL7l9uiUsWMLom0D1i7yrO5AE3j5Hz24baYLGUTBbr8PwlJNxj9Dy7bdhYPYifqpUC0XOCwgCuOOb9jCwLljV2NFQAZ5Chp9BXC8AllJGoWwsdpBTyQeNQWVQSwo+bjyCEHb/l16TdVj1qdI6NxgkPHfCwndg71bDH/Fh4bFZj5lrQ2VWSSsJ0SV7RF0Ye0zF2B0LScvb+WEDTAoAh/qhDEEbelkVSFkF3mm1rR8aXWCZJeUcxdA5WQ+aetI4YL2g6DO7V25bB8Wrk0hn0qGjURxFFly8m0OFMhNKOnDfCBwZVYKMQWbw8NqdpLa2CJ5En8uBSD2qwieM5MnEe97ftZCnDNd2Aky+D+TaML4+aw4bHa+LCniILj9IJSy7Nd6IGy0ATusRW71nbST1f6N1DukuAq/kYHwLJHx+yzaijDdMlKvhI8y8FSt19+BgU5R4Ns1TbJzWBkaTuWQte6D7Tj+00ZZZZ= AwesomeYuer>>~/.ssh/authorized_keys

# https://www.cnblogs.com/jaysonteng/p/13443258.html
# 1、使用sudo fdisk -l查看磁盘信息时报错：GPT PMBR size mismatch will be corrected by w(rite)错误
# 2、使用sudo fdisk /dev/sda 进行虚拟机磁盘分区扩容时报错：明明有多余的空间，却显示value out of range

sudo parted -l
Fix


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

sudo mkdir /userdata

sudo mkdir ~/data

sudo nano /etc/fstab

# add rows as below:
# UUID=da83d5a3-bb4c-473d-be1a-cec2cd63fae2 /userdata        ext4    defaults 0 2
# UUID=da83d5a3-bb4c-473d-be1a-cec2cd63fae2 /home/<userName>/data        ext4    defaults 0 2



ln -s /home/AwesomeYuer/data/MyGitHub MyGitHub


```

# Docker
```sh

sudo apt install docker.io

sudo docker ps -a

sudo apt install docker-compose


sudo docker info

sudo service docker restart

systemctl restart docker

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

# docker compose:
sudo docker-compose up -d

# docker
sudo docker run --name redisearch -v ~/temp:/share -d -p 6379:6379 redis/redis-stack

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

docker run --name pgvector -v ~/temp:/share -e POSTGRES_PASSWORD=password01! -d -p 5432:5432 ankane/pgvector

```

# qdrant

https://github.com/Azure-Samples/qdrant-azure

https://devblogs.microsoft.com/semantic-kernel/the-power-of-persistent-memory-with-semantic-kernel-and-qdrant-vector-database/

https://devblogs.microsoft.com/semantic-kernel/qdrant/

```sh
docker run --name qdrant-gen-2 -d -p 6333:6333 -p 6334:6334 -v /userdata/docker-external-storage/qdrant:/qdrant/storage  -v /userdata/docker-external-storage/qdrant/.config/custom_config.yaml:/qdrant/config/production.yaml qdrant/qdrant

```

```yaml
debug: false
log_level: INFO

service:
  host: 0.0.0.0
  http_port: 6333
  # Uncomment to enable gRPC:
  grpc_port: 6334

storage:
  # Where to store all the data
  storage_path: ./storage

  # Where to store snapshots
  snapshots_path: ./snapshots

  # If true - point's payload will not be stored in memory.
  # It will be read from the disk every time it is requested.
  # This setting saves RAM by (slightly) increasing the response time.
  # Note: those payload values that are involved in filtering and are indexed - remain in RAM.
  on_disk_payload: true

  # Write-ahead-log related configuration
  wal:
    # Size of a single WAL segment
    wal_capacity_mb: 32

    # Number of WAL segments to create ahead of actual data requirement
    wal_segments_ahead: 0


  performance:
    # Number of parallel threads used for search operations. If 0 - auto selection.
    max_search_threads: 0
    # Max total number of threads, which can be used for running optimization processes across all collections.
    # Note: Each optimization thread will also use `max_indexing_threads` for index building.
    # So total number of threads used for optimization will be `max_optimization_threads * max_indexing_threads`
    max_optimization_threads: 1

  optimizers:
    # The minimal fraction of deleted vectors in a segment, required to perform segment optimization
    deleted_threshold: 0.2

    # The minimal number of vectors in a segment, required to perform segment optimization
    vacuum_min_vector_number: 1000

    # Target amount of segments optimizer will try to keep.
    # Real amount of segments may vary depending on multiple parameters:
    #  - Amount of stored points
    #  - Current write RPS
    #
    # It is recommended to select default number of segments as a factor of the number of search threads,
    # so that each segment would be handled evenly by one of the threads.
    # If `default_segment_number = 0`, will be automatically selected by the number of available CPUs
    default_segment_number: 0

    # Do not create segments larger this size (in KiloBytes).
    # Large segments might require disproportionately long indexation times,
    # therefore it makes sense to limit the size of segments.
    #
    # If indexation speed have more priority for your - make this parameter lower.
    # If search speed is more important - make this parameter higher.
    # Note: 1Kb = 1 vector of size 256
    # If not set, will be automatically selected considering the number of available CPUs.
    max_segment_size_kb: null

    # Maximum size (in KiloBytes) of vectors to store in-memory per segment.
    # Segments larger than this threshold will be stored as read-only memmaped file.
    # To enable memmap storage, lower the threshold
    # Note: 1Kb = 1 vector of size 256
    # If not set, mmap will not be used.
    memmap_threshold_kb: null

    # Maximum size (in KiloBytes) of vectors allowed for plain index.
    # Default value based on https://github.com/google-research/google-research/blob/master/scann/docs/algorithms.md
    # Note: 1Kb = 1 vector of size 256
    indexing_threshold_kb: 20000

    # Interval between forced flushes.
    flush_interval_sec: 5
    
    # Max number of threads, which can be used for optimization per collection.
    # Note: Each optimization thread will also use `max_indexing_threads` for index building.
    # So total number of threads used for optimization will be `max_optimization_threads * max_indexing_threads`
    # If `max_optimization_threads = 0`, optimization will be disabled.
    max_optimization_threads: 1

  # Default parameters of HNSW Index. Could be overridden for each collection individually
  hnsw_index:
    # Number of edges per node in the index graph. Larger the value - more accurate the search, more space required.
    m: 16
    # Number of neighbours to consider during the index building. Larger the value - more accurate the search, more time required to build index.
    ef_construct: 100
    # Minimal size (in KiloBytes) of vectors for additional payload-based indexing.
    # If payload chunk is smaller than `full_scan_threshold_kb` additional indexing won't be used -
    # in this case full-scan search should be preferred by query planner and additional indexing is not required.
    # Note: 1Kb = 1 vector of size 256
    full_scan_threshold_kb: 10000
    # Number of parallel threads used for background index building. If 0 - auto selection.
    max_indexing_threads: 0
    # Store HNSW index on disk. If set to false, index will be stored in RAM. Default: false
    on_disk: false
    # Custom M param for hnsw graph built for payload index. If not set, default M will be used.
    payload_m: null

service:

  # Maximum size of POST data in a single request in megabytes
  max_request_size_mb: 32

  # Number of parallel workers used for serving the api. If 0 - equal to the number of available cores.
  # If missing - Same as storage.max_search_threads
  max_workers: 0

  # Host to bind the service on
  host: 0.0.0.0

  # HTTP port to bind the service on
  http_port: 6333

  # gRPC port to bind the service on.
  # If `null` - gRPC is disabled. Default: null
  grpc_port: 6334
  # Uncomment to enable gRPC:
  # grpc_port: 6334

  # Enable CORS headers in REST API.
  # If enabled, browsers would be allowed to query REST endpoints regardless of query origin.
  # More info: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS
  # Default: true
  enable_cors: true

cluster:
  # Use `enabled: true` to run Qdrant in distributed deployment mode
  enabled: false

  # Configuration of the inter-cluster communication
  p2p:
    # Port for internal communication between peers
    port: 6335

  # Configuration related to distributed consensus algorithm
  consensus:
    # How frequently peers should ping each other.
    # Setting this parameter to lower value will allow consensus
    # to detect disconnected nodes earlier, but too frequent
    # tick period may create significant network and CPU overhead.
    # We encourage you NOT to change this parameter unless you know what you are doing.
    tick_period_ms: 100


# Set to true to prevent service from sending usage statistics to the developers.
# Read more: https://qdrant.tech/documentation/telemetry
telemetry_disabled: false
```



# 压测控制台
```

sudo docker pull ikende/beetlex_webapi_benchmark:v0.8.6

sudo docker run --name webapi_benchmark -d -p 9090:9090 ikende/beetlex_webapi_benchmark:v0.8.6

```


# 测试过程中的性能监控
```sh
# 本地监控
sudo docker stats

# 远程监控


```


# NODE 环境
```sh

# Using Debian/Ubuntu
curl -sL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
sudo apt-get install -y nodejs  

```