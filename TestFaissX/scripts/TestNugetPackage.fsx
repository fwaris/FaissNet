//test nuget package create here, locally
//need nuget config for local packages
#r "nuget: FaissNet"

let m1 = new FaissNet.IndexFlat(20,FaissNet.MetricType.METRIC_L2)
let v1 = [|1..20|] |> Array.map(fun i -> if i % 2 = 0 then 1.0f else -1.0f)
m1.Add([|v1|])
let count = m1.Count()
m1.Dispose()

