# FaissNet
A [faiss](https://github.com/facebookresearch/faiss) wrapper in dotnet.

Faiss is essentially a vector store for efficiently searching the nearest neighbors of a given set of vectors.

The primary use is for Generative AI applications, e.g. ChatGPT question and answering; [Semantic Kernel](https://github.com/microsoft/semantic-kernel); etc.

Current versions is for x64 Windows, however, as the underlying native wrapper is built with C++/CMake, support for other platforms is possible.

Example:

```F#
#r "nuget: FaissNet"
let d = 64
let sz = 10
use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
let data = [|for i in 1 .. sz -> [| for j in 1 .. d -> if j%2 = 0 then -1.f else 1.f  |]|] 
let ids = [|for i in 1 .. sz -> int64 i|]
idx.AddWithIds(data,ids)
let srchVec = [|for i in 1 .. sz -> 1.f |]
let nbrDists,nbrIds = idx.Search([|srchVec|],2)
printfn $"nbrIds: {nbrIds}"
idx.Dispose()
```

See TestFaissX project for example usage.


