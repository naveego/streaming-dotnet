#!/usr/bin/env bash

version=$(cat VERSION)

echo "##teamcity[setParameter name='env.VERSION_NUMBER' value='${version}']"
