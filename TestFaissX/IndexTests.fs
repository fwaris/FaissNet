module IndexTests

open System
open System.IO
open Xunit

let DIM = 1536

[<Fact>]
let IndexAddWithIds() =
    let d = DIM
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d 100
    let ids = TestUtils.randIdArray 100
    idx.AddWithIds(data,ids)
    Assert.True(true)

[<Fact>]
let IndexTestAdd() =
    let d = DIM
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d 100
    idx.Add(data);
    Assert.True(true)

//[<Fact>]
//let IndexFlatL2Test() =
//    let d = 64
//    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.baseIndexTest d idx        

//[<Fact>]
//let IndexHSNWTest() =
//    let d = 64
//    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
//    TestUtils.baseIndexTest d idx        


//[<Fact>]
//let IndexFlatWriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexFlat> d idx

//[<Fact>]
//let IndexFlatL2WriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexFlatL2> d idx

//[<Fact>]
//let IndexHSNWWriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexHNSWFlat> d idx

(*
//these throw c++ exceptions which are not handled well by the testing infrastructure
[<Fact>]
let IndexFlatAddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx

[<Fact>]
let IndexFlatL2AddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx

[<Fact>]
let IndexHSNWAddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx
*)


(*
//removal not supported for HSNW
[<Fact>]
let IndexHSNWAddRemoveIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.addRemoveIds d idx
*)
