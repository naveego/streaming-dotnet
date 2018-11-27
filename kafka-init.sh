#!/usr/bin/env bash
# d exec kafka-n1 chmod +x kafka-init.sh
# d exec kafka-n1 ./kafka-init.sh

#echo  "INITIALIZING KAKFA: STARTING KAFKA"
#
#echo  "INITIALIZING KAKFA: WAITING FOR KAFKA TO START"
#KAFKA_READY=$(false)
#
#until $KAFKA_READY; do
#  echo "INITIALIZING KAKFA: Kafka is unavailable - waiting"
#  sleep 1
#  BROKERS=$(echo dump | nc localhost 2181 | grep brokers)
#  if [[ $BROKERS == *"/brokers/ids/0"* ]]; then
#    KAFKA_READY=$(true)
#  fi
#done
#
#echo "INITIALIZING KAKFA: Creating Topics"

KAFKA_HOME=$(find . -name "*kafka*" -not -name "*logs*" -type d -print -quit)

$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic dotnet-streaming-1 
    
$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic dotnet-streaming-2 
    
$KAFKA_HOME/bin/kafka-topics.sh \
    --create --zookeeper localhost:2181 \
    --replication-factor 1 \
    --partitions 16 \
    --topic dotnet-streaming-3
