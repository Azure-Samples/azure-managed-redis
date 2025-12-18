package main

import (
	"context"
	"crypto/tls"
	"fmt"
	"log"
	"time"

	entraid "github.com/redis/go-redis-entraid"
	"github.com/redis/go-redis/v9"
)

func main() {
	ctx := context.Background()

	// Set your Redis host (hostname:port)
	redisHost := "contosopy24.<region>.redis.azure.net:10000"

	// Create a credentials provider using DefaultAzureCredential
	provider, err := entraid.NewDefaultAzureCredentialsProvider(entraid.DefaultAzureCredentialsProviderOptions{})

	if err != nil {
		log.Fatalf("Failed to create credentials provider: %v", err)
	}

	// Create Redis client with Entra ID authentication
	client := redis.NewClient(&redis.Options{
		Addr:                         redisHost,
		TLSConfig:                    &tls.Config{MinVersion: tls.VersionTLS12},
		WriteTimeout:                 5 * time.Second,
		StreamingCredentialsProvider: provider,
	})
	defer client.Close()

	// Ping the Redis server to test the connection
	pong, err := client.Ping(ctx).Result()
	if err != nil {
		log.Fatal("Failed to connect to Redis:", err)
	}
	fmt.Println("Ping returned: ", pong)

	// Do something with Redis and a key-value pair
	result, err := client.Set(ctx, "Message", "Hello, The cache is working with Go!", 0).Result()
	if err != nil {
		log.Fatal("SET Message failed:", err)
	}
	fmt.Println("SET Message succeeded:", result)

	value, err := client.Get(ctx, "Message").Result()
	if err != nil {
		if err == redis.Nil {
			fmt.Println("GET Message returned: key does not exist")
		} else {
			log.Fatal("GET Message failed:", err)
		}
	} else {
		fmt.Println("GET Message returned:", value)
	}

}
