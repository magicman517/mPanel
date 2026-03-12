[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace mPanel.Tests.API;

[CollectionDefinition(nameof(AspireFixture))]
public class AspireCollection : ICollectionFixture<AspireFixture>;