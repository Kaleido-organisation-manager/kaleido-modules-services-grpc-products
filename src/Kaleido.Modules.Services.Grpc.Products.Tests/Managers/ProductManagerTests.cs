using Kaleido.Modules.Services.Grpc.Products.Managers;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Managers
{
    [TestClass]
    public class ProductsManagerTests
    {
        private IProductsManager _productsManager = null!;
        private AutoMocker _mocker = new AutoMocker();

        [TestInitialize]
        public void Initialize()
        {
            _productsManager = _mocker.CreateInstance<ProductsManager>();
        }

        [TestMethod]
        public async Task GetProductAsync_ShouldReturnProduct_WhenExists()
        {
            // Arrange
            var productKey = Guid.NewGuid();
            var productEntity = new ProductEntity() { Key = productKey, Name = "Test Product", CategoryKey = Guid.NewGuid() }; // Initialize as necessary
            var productPrices = new List<ProductPriceEntity>(); // Initialize as necessary
            var expectedProduct = new Product(); // Initialize with expected properties

            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.GetAsync(productKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productEntity);
            _mocker.GetMock<IProductPricesRepository>()
                .Setup(repo => repo.GetAllByProductIdAsync(productKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productPrices);
            _mocker.GetMock<IProductMapper>()
                .Setup(mapper => mapper.FromEntities(productEntity, productPrices))
                .Returns(expectedProduct);

            // Act
            var result = await _productsManager.GetProductAsync(productKey.ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedProduct, result);
        }

        [TestMethod]
        public async Task GetProductAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productKey = "non-existing-product-key";
            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductEntity?)null);

            // Act
            var result = await _productsManager.GetProductAsync(productKey);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllProductsAsync_ShouldReturnProducts_WhenProductsExist()
        {
            // Arrange
            var productEntities = new List<ProductEntity>
            {
                new ProductEntity { Key = Guid.NewGuid(), CategoryKey = Guid.NewGuid(), Name = "Product 1" },
                new ProductEntity { Key = Guid.NewGuid(), CategoryKey = Guid.NewGuid(), Name = "Product 2" }
            };
            var productPrices = new List<ProductPriceEntity>(); // Initialize as necessary
            var expectedProducts = new List<Product> { new Product(), new Product() }; // Initialize with expected properties

            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productEntities);
            _mocker.GetMock<IProductPricesRepository>()
                .Setup(repo => repo.GetAllByProductIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productPrices);
            _mocker.GetMock<IProductMapper>()
                .Setup(mapper => mapper.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
                .Returns(expectedProducts.First());

            // Act
            var result = await _productsManager.GetAllProductsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedProducts.Count, result.Count());
        }

        [TestMethod]
        public async Task GetAllProductsByCategoryIdAsync_ShouldReturnProducts_WhenValidCategoryKey()
        {
            // Arrange
            var categoryKey = Guid.NewGuid();
            var productEntities = new List<ProductEntity>
            {
                new ProductEntity { Key = Guid.NewGuid(), CategoryKey = categoryKey, Name = "Product 1" },
                new ProductEntity { Key = Guid.NewGuid(), CategoryKey = categoryKey, Name = "Product 2" }
            };
            var productPrices = new List<ProductPriceEntity>(); // Initialize as necessary
            var expectedProducts = new List<Product> { new Product(), new Product() }; // Initialize with expected properties

            _mocker.GetMock<ICategoryValidator>()
                .Setup(v => v.ValidateIdAsync(categoryKey, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.GetAllByCategoryIdAsync(categoryKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productEntities);
            _mocker.GetMock<IProductPricesRepository>()
                .Setup(repo => repo.GetAllByProductIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productPrices);
            _mocker.GetMock<IProductMapper>()
                .Setup(mapper => mapper.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
                .Returns(new Product());

            // Act
            var result = await _productsManager.GetAllProductsByCategoryIdAsync(categoryKey);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(productEntities.Count, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task GetAllProductsByCategoryIdAsync_ShouldThrowValidationException_WhenCategoryIsInvalid()
        {
            // Arrange
            var categoryKey = Guid.NewGuid();
            _mocker.GetMock<ICategoryValidator>()
                .Setup(v => v.ValidateIdAsync(categoryKey, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Invalid category"));

            // Act
            await _productsManager.GetAllProductsByCategoryIdAsync(categoryKey);
        }

        [TestMethod]
        public async Task CreateProductAsync_ShouldCreateProduct_WhenValid()
        {
            // Arrange
            var productKey = Guid.NewGuid().ToString();
            var createProduct = new CreateProduct { CategoryKey = productKey, Description = "Test product" };
            var product = new Product { CategoryKey = createProduct.CategoryKey, Description = createProduct.Description, Key = productKey, Prices = { new List<ProductPrice>() } }; // Initialize as necessary
            var createdProductEntity = new ProductEntity() { Key = Guid.Parse(product.Key), CategoryKey = Guid.Parse(createProduct.CategoryKey), Name = createProduct.Name }; // Initialize as necessary

            _mocker.GetMock<ICategoryValidator>()
                .Setup(v => v.ValidateIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductValidator>()
                .Setup(v => v.ValidateCreateAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductPriceValidator>()
                .Setup(v => v.ValidateAsync(product.Prices, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductMapper>()
                .Setup(mapper => mapper.ToCreateEntity(product, It.IsAny<int>())).Returns(createdProductEntity);
            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.CreateAsync(createdProductEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProductEntity);
            _mocker.GetMock<IProductPricesRepository>()
                .Setup(repo => repo.CreateRangeAsync(It.IsAny<List<ProductPriceEntity>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<ProductPriceEntity> s, CancellationToken token) => s);
            _mocker.GetMock<IProductMapper>()
                .Setup(s => s.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
                .Returns(product);

            // Act
            var result = await _productsManager.CreateProductAsync(createProduct);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(product.Key, result.Key);
            _mocker.GetMock<IProductsRepository>()
                .Verify(repo => repo.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task CreateProductAsync_ShouldThrowValidationException_WhenCategoryIsInvalid()
        {
            // Arrange
            var createProduct = new CreateProduct { CategoryKey = "invalid-category", Description = "Test product" };
            _mocker.GetMock<ICategoryValidator>()
                .Setup(v => v.ValidateIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Invalid category"));

            // Act
            await _productsManager.CreateProductAsync(createProduct);
        }

        [TestMethod]
        public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValid()
        {
            // Arrange
            var productKey = Guid.NewGuid();
            var productKeyString = productKey.ToString();

            var categoryKey = Guid.NewGuid();
            var categoryKeyString = categoryKey.ToString();
            var product = new Product { Key = productKeyString, CategoryKey = categoryKeyString, Name = "Test Product" };
            var storedProductEntity = new ProductEntity { Key = productKey, Revision = 1, CategoryKey = categoryKey, Name = product.Name };
            var updatedProductEntity = new ProductEntity { Key = productKey, Revision = 2, CategoryKey = categoryKey, Name = product.Name };

            _mocker.GetMock<IProductValidator>()
                .Setup(v => v.ValidateUpdateAsync(product, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<ICategoryValidator>()
                .Setup(v => v.ValidateIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductPriceValidator>()
                .Setup(v => v.ValidateAsync(product.Prices, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedProductEntity);
            _mocker.GetMock<IProductMapper>()
                .Setup(mapper => mapper.ToCreateEntity(product, 2)).Returns(updatedProductEntity);
            _mocker.GetMock<IProductsRepository>()
                .Setup(repo => repo.UpdateAsync(updatedProductEntity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedProductEntity);
            _mocker.GetMock<IProductPricesRepository>()
                .Setup(repo => repo.CreateRangeAsync(It.IsAny<List<ProductPriceEntity>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<ProductPriceEntity> s, CancellationToken token) => s);
            _mocker.GetMock<IProductMapper>()
                .Setup(s => s.FromEntities(updatedProductEntity, It.IsAny<IEnumerable<ProductPriceEntity>>()))
                .Returns(product);

            // Act
            var result = await _productsManager.UpdateProductAsync(product);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(product.Key, result.Key);
            _mocker.GetMock<IProductsRepository>()
                .Verify(repo => repo.UpdateAsync(updatedProductEntity, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task UpdateProductAsync_ShouldThrowValidationException_WhenProductIsInvalid()
        {
            // Arrange
            var product = new Product { Key = "existing-product-key", CategoryKey = "invalid-category" };
            _mocker.GetMock<IProductValidator>()
                .Setup(v => v.ValidateUpdateAsync(product, It.IsAny<CancellationToken>())).ThrowsAsync(new ValidationException("Invalid product"));

            // Act
            await _productsManager.UpdateProductAsync(product);
        }

    }
}
