//test nuget package create here, locally
//need nuget config for local packages
#r "nuget: FaissNet"

let idx1 = new FaissNet.IndexFlat(20,FaissNet.MetricType.METRIC_L2)
let v1 = [|1..20|] |> Array.map(fun i -> if i % 2 = 0 then 1.0f else -1.0f)
idx1.Add([|v1|])
let count = idx1.Count()
let nbrs,dists = idx1.Search([|v1|],1) //find neighbors
idx1.Dispose()

