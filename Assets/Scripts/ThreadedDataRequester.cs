using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour {

    // Singleton instance of ThreadedDataRequester
    static ThreadedDataRequester instance;
    
    // Queue to hold thread information for later processing in the main thread
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    // Called when the script is initialized, used for setting up the singleton instance
    void Awake() {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    // Static method to request data in a separate thread and execute a callback when done
    public static void RequestData(Func<object> generateData, Action<object> callback) {
        // Create a ThreadStart delegate to handle the thread execution
        ThreadStart threadStart = delegate {
            // Run the DataThread method on a new thread
            instance.DataThread(generateData, callback);
        };

        // Start the new thread
        new Thread(threadStart).Start();
    }

    // Method executed on a separate thread to generate the requested data and enqueue it for the main thread
    void DataThread(Func<object> generateData, Action<object> callback) {
        // Generate the data using the provided delegate
        object data = generateData();
        
        // Lock the data queue to ensure thread-safe access
        lock (dataQueue) {
            // Enqueue the callback and the generated data as a new ThreadInfo
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    // Update is called once per frame. It processes the data queue and invokes the callbacks on the main thread
    void Update() {
        // Check if there is any data to process in the queue
        if (dataQueue.Count > 0) {
            // Process all items in the queue
            for (int i = 0; i < dataQueue.Count; i++) {
                // Dequeue the next thread info and execute the callback
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    // Struct to hold the callback and the data to pass to the callback
    struct ThreadInfo {
        public readonly Action<object> callback;  // Callback to be executed with the data
        public readonly object parameter;         // Data to pass to the callback

        // Constructor for initializing ThreadInfo with a callback and parameter
        public ThreadInfo(Action<object> callback, object parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}