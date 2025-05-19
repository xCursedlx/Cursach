using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace ProductManage.Data
{
    /// <summary>
    /// Единица работы для управления транзакциями и репозиториями
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        private CategoryRepository _categoryRepository;
        private ProductRepository _productRepository;
        private SupplierRepository _supplierRepository;
        private SupplyItemRepository _supplyItemRepository;
        private SupplyRepository _supplyRepository;
        private UserRepository _userRepository;
        private FinancialRepository _financialRepository;

        public UnitOfWork()
        {
            _connection = DatabaseManager.GetConnection();
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Репозиторий категорий
        /// </summary>
        public CategoryRepository Categories =>
            _categoryRepository ??= new CategoryRepository(_connection);

        /// <summary>
        /// Репозиторий товаров
        /// </summary>
        public ProductRepository Products =>
            _productRepository ??= new ProductRepository();

        /// <summary>
        /// Репозиторий поставщиков
        /// </summary>
        public SupplierRepository Suppliers =>
            _supplierRepository ??= new SupplierRepository(_connection);

        /// <summary>
        /// Репозиторий элементов поставки
        /// </summary>
        public SupplyItemRepository SupplyItems =>
            _supplyItemRepository ??= new SupplyItemRepository(_connection);

        /// <summary>
        /// Репозиторий поставок
        /// </summary>
        public SupplyRepository Supplies =>
            _supplyRepository ??= new SupplyRepository(_connection);

        /// <summary>
        /// Репозиторий пользователей
        /// </summary>
        public UserRepository Users =>
            _userRepository ??= new UserRepository(_connection);

        /// <summary>
        /// Репозиторий финансовых операций
        /// </summary>
        public FinancialRepository FinancialOperations =>
            _financialRepository ??= new FinancialRepository(_connection);

        /// <summary>
        /// Подтверждение всех изменений
        /// </summary>
        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                ResetRepositories();
            }
        }

        /// <summary>
        /// Откат всех изменений
        /// </summary>
        public void Rollback()
        {
            _transaction.Rollback();
            ResetRepositories();
        }

        private void ResetRepositories()
        {
            _categoryRepository = null;
            _productRepository = null;
            _supplierRepository = null;
            _supplyItemRepository = null; 
            _supplyRepository = null;
            _userRepository = null;
            _financialRepository = null;
        }


        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}