# FaissNet
A [faiss](https://github.com/facebookresearch/faiss) wrapper in dotnet, written in C++/CLI.

Faiss is essentially a vector store for efficiently searching the nearest neighbors of a given set of vectors.

The primary use is for Generative AI applications, e.g. ChatGPT question and answering; [Semantic Kernel](https://github.com/microsoft/semantic-kernel); etc.

Current versions is for x64 Windows. (Other wrappers for linux avaiable in nuget).

Example:

```F#
#r "nuget: FaissNet"
let idx1 = new FaissNet.IndexFlat(20,FaissNet.MetricType.METRIC_L2)
let v1 = [|1..20|] |> Array.map(fun i -> if i % 2 = 0 then 1.0f else -1.0f)
idx1.Add([|v1|]) //add vectors to store
let count = idx1.Count()
let nbrs,dists = idx1.Search([|v1|],1) //find neighbors
idx1.Dispose()
```


