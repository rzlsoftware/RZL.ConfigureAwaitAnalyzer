# RZL.ConfigureAwaitAnalyzer
A Roslyn analyzer to not let you forget to add `.ConfigureAwait(false)`

Helps you to ensure that all `await` calls are configured to not capture the actual context.  
That is important when calling `.Wait()` or `.Result` on a running `Task` in a WPF Application (excapt the last one in the `View/ViewModel`).
