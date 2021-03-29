using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

public abstract class WorkerThread : MonoBehaviour
{
    private Thread threadHandle;

    private EventWaitHandle updateWait = new EventWaitHandle(false, EventResetMode.AutoReset);

    protected bool IsRunning = true;

    protected float DeltaTime;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        this.Initialize();

        this.threadHandle = new Thread(() =>
        {
            this.UpdateWorkerThread();
        });
        this.threadHandle.IsBackground = true;

        this.threadHandle.Start();
    }

    // Update is called once per frame
    private void Update()
    {
        this.DeltaTime = Time.deltaTime;
        this.PreUpdate();
        this.updateWait.Set();
    }

    private void UpdateWorkerThread()
    {
        while (this.IsRunning)
        {
            this.updateWait.WaitOne();
            this.OnThreadUpdate();
        }
    }

    private void OnDestroy()
    {
        this.IsRunning = false;
    }
    
    protected virtual void Initialize(){}

    protected virtual void PreUpdate(){}

    protected abstract void OnThreadUpdate();
}
