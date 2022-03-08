Threads
===============

The 'Threads' namespace contains the following tools:
- 'APC.MonobehaviourCall' implements the reactor design pattern
  to demultiplex threads execution into the Unity main threads.
  It allows execution from other threads to call Unity methods
  asynchronously by moving back execution temporarily into the
  Unity main thread.
- DataFlow is a asynchronous pipeline framework for data
  transformation. It provides tools for efficient creation of
  complex pipelines, with multiplexing and barriers.
- 'Pool' is a thread pool that can be used for applications
  requiring efficient parallel tasks.
- 'Result' defines futures. They can be used to be notified of
  asynchronous tasks completion, return value and potential
  exception errors.
- 'Task' is a wrapper that associate a callback to execute with
  a link 'Result' corresponding to the execution state on this
  callback.
- 'Thread' is a wrapper around threads implementation that is
  usable on platforms including hololens.
- 'Workers' is the base class of the thread pool. It defines a
  set of threads that can be used for any purposes.
