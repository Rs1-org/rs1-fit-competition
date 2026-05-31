using AutoMapper;
using Moq;

namespace Competition.Tests.Infrastructure;

public abstract class BaseUnitTest
{
    protected readonly Mock<IMapper> MapperMock;
    protected IMapper Mapper => MapperMock.Object;
    
    protected BaseUnitTest()
    {
        MapperMock = new Mock<IMapper>();
    }
}
