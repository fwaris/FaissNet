module IdMapTests
open System
open System.IO
open Xunit

let DIM = 1536

//let addDataWithIdMap d (idx:FaissNet.CreateIndex) = 
//    let sz = 100
//    let data = TestUtils.randFloatArray d sz
//    let ids = TestUtils.randIdArray sz
//    use idMap = new FaissNet.IdMap(idx) 
//    idMap.AddWithIds(data,ids)
//    let d1 = data.[0..1]
//    let id1 = ids.[0..1]
//    let nbrIds,nbrVecs = idMap.Search(d1,1)
//    Assert.True((nbrIds.[0].[0]=id1.[0]))

//let addRemoveIds d (idx:FaissNet.CreateIndex) =
//    let sz = 100
//    let data = TestUtils.randFloatArray d sz
//    let ids = TestUtils.randIdArray sz
//    use idMap = new FaissNet.IdMap(idx) 
//    idMap.AddWithIds(data,ids)
//    let c1 = idMap.Count()
//    let remIds = ids.[0..sz/2]
//    let vecs = idMap.Reconstruct(remIds[0..1]) // this should work
//    idMap.Remove(remIds)
//    let c2 = idMap.Count()
//    Assert.True((c2 = c1 - remIds.Length))
//    //let tryRemove() = idMap.Reconstruct(remIds[0..1]) |> ignore               //this should fail as ids have been removed
//    //Assert.Throws<System.Runtime.InteropServices.SEHException>(tryRemove)     //causes problems with testing host

let saveLoadIndex d (idMap:FaissNet.Index)=
    let sz = 100
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.randIdArray sz
    idMap.AddWithIds(data,ids)
    let c1 = idMap.Count()
    let fn = Path.GetTempFileName()
    FaissNet.Instance.WriteIndex(idMap,fn)
    use idMap2 = FaissNet.Instance.ReadIndex(fn)
    Assert.True((c1=idMap2.Count()))

[<Fact>]
let IndexBaseTest() = 
    use idx = FaissNet.Factory.Default(DIM,FaissNet.MetricType.METRIC_L2)
    saveLoadIndex DIM idx

[<Fact>]
let CatchErrorTest() = 
    let act() =
        try
            use x = FaissNet.Factory.Create(DIM,"BAD STRING",FaissNet.MetricType.METRIC_L2)
            ()
        with ex -> 
            raise ex
    Assert.Throws<Exception>(act)
    


//[<Fact>]
//let IndexFlatAddRemoveIdsTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
//    addRemoveIds d idx

//[<Fact>]
//let IndexFlatL2AddRemoveIdsTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
//    addRemoveIds d idx

//[<Fact>]
//let IndexFlatAddWithIdMapTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
//    addDataWithIdMap d idx

//[<Fact>]
//let IndexFlatL2AddWithIdMapTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
//    addDataWithIdMap d idx

//[<Fact>]
//let IndexHSNWAddWithIdMapTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2))
//    addDataWithIdMap d idx

//[<Fact>]
//let IndexFlatAddWithIdMapSaveLoadTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
//    let idMap = new FaissNet.IdMap(idx)
//    saveLoadIdMap d idMap

//[<Fact>]
//let IndexFlatL2AddWithIdMapSaveLoadTest() =
//    let d = DIM
//    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
//    let idMap = new FaissNet.IdMap(idx)
//    saveLoadIdMap d idMap

////[<Fact>]
////let IndexHSNWAddWithIdMapSaveLoadTest() =
////    let d = DIM
////    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2))
////    let idMap = new FaissNet.IdMap(idx)
////    saveLoadIdMap d idMap

