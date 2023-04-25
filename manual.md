# ���������
```sh
# ����sshԶ�̵�¼������

# �ͻ�������

ssh-keygen -t rsa

# �ͻ��˴����� Windows: C:\Users\<userName>\.ssh\id_rsa.pub �ļ�
# �༭������������Զ�� linux �����ִ�к󼴿�����Զ�� ssh

echo ssh-rsa ZZZZB3NzaC1yc2EAAAADAQABAAABgQCoOqAxklUB39B6QbT4zIgTc+bAf2CrICZ/saL7l9uiUsWMLom0D1i7yrO5AE3j5Hz24baYLGUTBbr8PwlJNxj9Dy7bdhYPYifqpUC0XOCwgCuOOb9jCwLljV2NFQAZ5Chp9BXC8AllJGoWwsdpBTyQeNQWVQSwo+bjyCEHb/l16TdVj1qdI6NxgkPHfCwndg71bDH/Fh4bFZj5lrQ2VWSSsJ0SV7RF0Ye0zF2B0LScvb+WEDTAoAh/qhDEEbelkVSFkF3mm1rR8aXWCZJeUcxdA5WQ+aetI4YL2g6DO7V25bB8Wrk0hn0qGjURxFFly8m0OFMhNKOnDfCBwZVYKMQWbw8NqdpLa2CJ5En8uBSD2qwieM5MnEe97ftZCnDNd2Aky+D+TaML4+aw4bHa+LCniILj9IJSy7Nd6IGy0ATusRW71nbST1f6N1DukuAq/kYHwLJHx+yzaijDdMlKvhI8y8FSt19+BgU5R4Ns1TbJzWBkaTuWQte6D7Tj+00ZZZZ= AwesomeYuer>>~/.ssh/authorized_keys

# https://learn.microsoft.com/en-us/azure/virtual-machines/linux/attach-disk-portal?tabs=ubuntu

# Find the disk ���Ҽ��ظ��Ӵ���
# Once connected to your VM, you need to find the disk. In this example, we're using lsblk to list the disks.
# ���ӵ� VM ������Ҫ�ҵ����̡��ڴ�ʾ���У�����ʹ�� lsblk �г����̡�

lsblk -o NAME,HCTL,SIZE,MOUNTPOINT | grep -i "sd"

# Prepare a new empty disk ׼���µĿ���
# The following example uses parted on /dev/sdc, which is where the first data disk will typically be on most VMs. Replace sdc with the correct option for your disk. We're also formatting it using the XFS filesystem.
# The following example uses parted on /dev/sdc, which is where the first data disk will typically be on most VMs. Replace sdc with the correct option for your disk. We're also formatting it using the XFS filesystem.
# ����ʾ���� /dev/sdc ��ʹ�� parted �����ǵ�һ�����ݴ���ͨ��λ�ڴ���� VM �ϵ�λ�á��� sdc �滻Ϊ�ʺ����Ĵ��̵���ȷѡ����ǻ�ʹ�� XFS �ļ�ϵͳ������и�ʽ����Prepare a new empty disk ׼���µĿ���

sudo parted /dev/sdc --script mklabel gpt mkpart xfspart xfs 0% 100%
sudo mkfs.xfs /dev/sdc1
sudo partprobe /dev/sdc1

sudo fdisk -l

sudo apt-get install nfs-common

sudo mkfs -t ext4 /dev/sdc1

# Mount the disk ���ش���
# Create a directory to mount the file system using mkdir. The following example creates a directory at /datadrive:
# ʹ�� mkdir ����Ŀ¼�Թ����ļ�ϵͳ������ʾ���� /userdata��~/data ������һ��Ŀ¼

sudo mkdir /userdata
sudo mkdir ~/data

# Use mount to then mount the filesystem. The following example mounts the /dev/sdc1 partition to the /datadrive mount point:
# Ȼ��ʹ�� mount �����ļ�ϵͳ������ʾ���� /dev/sdc1 �������ص� /datadrive ���ص㣺

# sudo mount /dev/sdc1 /userdata
# sudo mount /dev/sdc1 ~/data

sudo chmod 777 ~/data
sudo chmod 777 /userdata


# ��������صĴ���ֻ���ڵ�ǰ����ϵͳ��������Ч��������������ᶪ�����ء�
# �����Զ�����
# �ҵ������̵� UUID
# ��������г����д��̵���Ϣ���� /dev/sda , /dev/sdb�� uuid ���������ʹ��grep ������Ҷ�Ӧ���̡�

ls -l /dev/disk/by-uuid

# ���� fstab �ļ�

cp /etc/fstab /etc/fstab.bak

# �޸� fstab �ļ�
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

# һ��ɾ�������Ѿ�ֹͣ������
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

# in redis-cli ������, ������ʧЧ, ��������
config set requirepass password01!

auth password01!

config get requirepass

quit

```

# Python3 ����
```
python3 --version

sudo apt install python3-pip

sudo python3 -m pip install ipykernel -U --user --force-reinstall

pip install numpy

pip install pandas

pip install nbutils

pip install wget

# ������ openai-cookbook-python/examples/vector_databases/redis/

wget https://cdn.openai.com/API/examples/data/vector_database_wikipedia_articles_embedded.zip

# vscode �� Dev ��֧����:
# openai-cookbook-python/examples/vector_databases/redis/RediSearchVectorQueries.ipynb

```

# PostgresSQL + PgVector + Docker
# ssh
```sh

docker pull ankane/pgvector

docker run --name test-pgvector -v ~/temp:/share -e POSTGRES_PASSWORD=password01! -d -p 5432:5432 ankane/pgvector

```

# ѹ�����̨
```

sudo docker pull ikende/beetlex_webapi_benchmark:v0.8.6

sudo docker run -d -p 9090:9090 ikende/beetlex_webapi_benchmark:v0.8.6

```


# ���Թ����е����ܼ��
```sh
# ���ؼ��
sudo docker stats

# Զ�̼��


```