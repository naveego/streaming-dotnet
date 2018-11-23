#!/usr/bin/env bash
# d exec kafka-n1 chmod +x kafka-init.sh
# d exec kafka-n1 ./kafka-init.sh

KAFKA_HOME=$(find . -name "*kafka*" -not -name "*logs*" -type d -print -quit)


$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic topic1 
    
$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic topic2
    
$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic topic3
