[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace mPanel.Tests.Web;

[CollectionDefinition(nameof(AspireFixture))]
public class AspireCollection : ICollectionFixture<AspireFixture>;