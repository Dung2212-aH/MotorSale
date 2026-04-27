import React, { useState, useEffect } from 'react';
import { productApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const emptyProduct = {
    productCode: '',
    name: '',
    slug: '',
    productType: 'Motorcycle',
    categoryId: '',
    brandId: '',
    carModelId: '',
    showroomId: '',
    basePrice: 0,
    salePrice: '',
    stockQuantity: 0,
    mainImageUrl: '',
    shortDescription: '',
    description: '',
    status: 'Available',
    isActive: true,
};

const Products = () => {
    const [products, setProducts] = useState([]);
    const [filters, setFilters] = useState({ categories: [], brands: [], carModels: [], showrooms: [] });
    const [loading, setLoading] = useState(true);
    const [query, setQuery] = useState({
        keyword: '',
        categoryId: '',
        productType: '',
        brandId: '',
        carModelId: '',
        status: '',
        sortBy: '',
    });
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [totalPages, setTotalPages] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [showModal, setShowModal] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);
    const [formData, setFormData] = useState(emptyProduct);
    const [error, setError] = useState('');
    const { isAdmin } = useAuth();

    useEffect(() => {
        loadFilters();
    }, []);

    useEffect(() => {
        loadProducts();
    }, [page, query]);

    const loadFilters = async () => {
        try {
            const response = await productApi.getFilters();
            setFilters(response.data || { categories: [], brands: [], carModels: [], showrooms: [] });
        } catch (error) {
            console.error('Failed to load product filters:', error);
        }
    };

    const loadProducts = async () => {
        setLoading(true);
        try {
            const response = await productApi.search({
                ...query,
                categoryId: query.categoryId || undefined,
                brandId: query.brandId || undefined,
                carModelId: query.carModelId || undefined,
                page,
                pageSize,
            });
            setProducts(response.data.items || []);
            setTotalPages(response.data.totalPages || 0);
            setTotalCount(response.data.totalCount || 0);
        } catch (error) {
            console.error('Failed to load products:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = (e) => {
        e.preventDefault();
        setPage(1);
        loadProducts();
    };

    const toSlug = (value) => value
        .trim()
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '');

    const openModal = (product = null) => {
        if (product) {
            setEditingProduct(product);
            setFormData({
                ...emptyProduct,
                ...product,
                categoryId: product.categoryId || '',
                brandId: product.brandId || '',
                carModelId: product.carModelId || '',
                showroomId: product.showroomId || '',
                salePrice: product.salePrice ?? '',
            });
        } else {
            setEditingProduct(null);
            setFormData({
                ...emptyProduct,
                categoryId: filters.categories[0]?.id || '',
                showroomId: filters.showrooms[0]?.id || '',
            });
        }
        setError('');
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingProduct(null);
        setError('');
    };

    const numberOrNull = (value) => value === '' || value === null || value === undefined ? null : Number(value);
    const intOrNull = (value) => value === '' || value === null || value === undefined ? null : parseInt(value, 10);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const data = {
                ...formData,
                slug: formData.slug || toSlug(formData.name),
                categoryId: parseInt(formData.categoryId, 10),
                brandId: intOrNull(formData.brandId),
                carModelId: intOrNull(formData.carModelId),
                showroomId: intOrNull(formData.showroomId),
                basePrice: Number(formData.basePrice),
                salePrice: numberOrNull(formData.salePrice),
                stockQuantity: parseInt(formData.stockQuantity, 10),
            };

            if (data.productType === 'Accessory') {
                data.brandId = null;
                data.carModelId = null;
            }

            if (editingProduct) {
                await productApi.update(editingProduct.id, data);
            } else {
                await productApi.create(data);
            }

            closeModal();
            loadProducts();
        } catch (error) {
            setError(error.response?.data?.message || 'Operation failed');
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Ẩn sản phẩm này khỏi website?')) return;

        try {
            await productApi.delete(id);
            loadProducts();
        } catch (error) {
            alert('Failed to hide product');
        }
    };

    const renderPagination = () => {
        const pages = [];
        for (let i = 1; i <= totalPages; i++) {
            pages.push(
                <li key={i} className={`page-item ${page === i ? 'active' : ''}`}>
                    <button className="page-link" onClick={() => setPage(i)}>{i}</button>
                </li>
            );
        }
        return pages;
    };

    const priceOf = (product) => product.salePrice ?? product.basePrice;
    const carModelsByBrand = filters.carModels.filter(model => !formData.brandId || model.brandId === Number(formData.brandId));

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Motorcycle & Accessory Catalog</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">
                    <div className="card">
                        <div className="card-header">
                            <form onSubmit={handleSearch}>
                                <div className="row">
                                    <div className="col-md-3 mb-2">
                                        <input
                                            type="text"
                                            className="form-control"
                                            placeholder="Search code, name, description"
                                            value={query.keyword}
                                            onChange={(e) => setQuery({ ...query, keyword: e.target.value })}
                                        />
                                    </div>
                                    <div className="col-md-2 mb-2">
                                        <select className="form-control" value={query.productType} onChange={(e) => setQuery({ ...query, productType: e.target.value })}>
                                            <option value="">All types</option>
                                            <option value="Motorcycle">Motorcycles</option>
                                            <option value="Accessory">Accessories</option>
                                        </select>
                                    </div>
                                    <div className="col-md-2 mb-2">
                                        <select className="form-control" value={query.categoryId} onChange={(e) => setQuery({ ...query, categoryId: e.target.value })}>
                                            <option value="">All categories</option>
                                            {filters.categories.map(cat => (
                                                <option key={cat.id} value={cat.id}>{cat.name}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="col-md-2 mb-2">
                                        <select className="form-control" value={query.brandId} onChange={(e) => setQuery({ ...query, brandId: e.target.value, carModelId: '' })}>
                                            <option value="">All brands</option>
                                            {filters.brands.map(brand => (
                                                <option key={brand.id} value={brand.id}>{brand.name}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="col-md-2 mb-2">
                                        <select className="form-control" value={query.status} onChange={(e) => setQuery({ ...query, status: e.target.value })}>
                                            <option value="">All status</option>
                                            <option value="Available">Available</option>
                                            <option value="Reserved">Reserved</option>
                                            <option value="Sold">Sold</option>
                                            <option value="Hidden">Hidden</option>
                                        </select>
                                    </div>
                                    <div className="col-md-1 mb-2">
                                        <button type="submit" className="btn btn-primary btn-block">
                                            <i className="fas fa-search"></i>
                                        </button>
                                    </div>
                                </div>
                            </form>
                            <div className="row mt-2">
                                <div className="col-md-3">
                                    <select className="form-control" value={query.sortBy} onChange={(e) => setQuery({ ...query, sortBy: e.target.value })}>
                                        <option value="">Newest</option>
                                        <option value="price_asc">Price low to high</option>
                                        <option value="price_desc">Price high to low</option>
                                        <option value="year_desc">Newest year</option>
                                        <option value="name_asc">Name A-Z</option>
                                    </select>
                                </div>
                                <div className="col-md-9 text-right">
                                    {isAdmin() && (
                                        <button className="btn btn-success" onClick={() => openModal()}>
                                            <i className="fas fa-plus"></i> Add Motorcycle / Accessory
                                        </button>
                                    )}
                                </div>
                            </div>
                        </div>
                        <div className="card-body">
                            {loading ? (
                                <div className="text-center py-5">
                                    <div className="spinner-border text-primary"></div>
                                </div>
                            ) : (
                                <>
                                    <table className="table table-bordered table-striped">
                                        <thead>
                                            <tr>
                                                <th>Code</th>
                                                <th>Name</th>
                                                <th>Type</th>
                                                <th>Brand / Model</th>
                                                <th>Price</th>
                                                <th>Stock</th>
                                                <th>Status</th>
                                                {isAdmin() && <th>Actions</th>}
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {products.length === 0 ? (
                                                <tr>
                                                    <td colSpan={isAdmin() ? 8 : 7} className="text-center">No products found</td>
                                                </tr>
                                            ) : (
                                                products.map(product => (
                                                    <tr key={product.id}>
                                                        <td>{product.productCode}</td>
                                                        <td>
                                                            <strong>{product.name}</strong>
                                                            <div className="text-muted small">{product.categoryName}</div>
                                                        </td>
                                                        <td>{product.productType}</td>
                                                        <td>
                                                            {product.brandName || '-'}
                                                            {product.carModelName && <span> / {product.carModelName}</span>}
                                                        </td>
                                                        <td>
                                                            {priceOf(product)?.toLocaleString()} VND
                                                            {product.salePrice && <div className="text-muted small"><s>{product.basePrice?.toLocaleString()} VND</s></div>}
                                                        </td>
                                                        <td>{product.stockQuantity}</td>
                                                        <td>
                                                            <span className={`badge ${product.status === 'Available' ? 'badge-success' : product.status === 'Sold' ? 'badge-danger' : 'badge-secondary'}`}>
                                                                {product.status}
                                                            </span>
                                                        </td>
                                                        {isAdmin() && (
                                                            <td>
                                                                <button className="btn btn-sm btn-info mr-1" onClick={() => openModal(product)}>
                                                                    <i className="fas fa-edit"></i>
                                                                </button>
                                                                <button className="btn btn-sm btn-danger" onClick={() => handleDelete(product.id)}>
                                                                    <i className="fas fa-eye-slash"></i>
                                                                </button>
                                                            </td>
                                                        )}
                                                    </tr>
                                                ))
                                            )}
                                        </tbody>
                                    </table>

                                    <div className="d-flex justify-content-between align-items-center">
                                        <span>Total: {totalCount} items</span>
                                        <nav>
                                            <ul className="pagination mb-0">
                                                <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                                                    <button className="page-link" onClick={() => setPage(page - 1)}>Previous</button>
                                                </li>
                                                {renderPagination()}
                                                <li className={`page-item ${page === totalPages || totalPages === 0 ? 'disabled' : ''}`}>
                                                    <button className="page-link" onClick={() => setPage(page + 1)}>Next</button>
                                                </li>
                                            </ul>
                                        </nav>
                                    </div>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </section>

            {showModal && (
                <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
                    <div className="modal-dialog modal-xl">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">{editingProduct ? 'Edit Catalog Item' : 'Add Catalog Item'}</h5>
                                <button type="button" className="close" onClick={closeModal}>
                                    <span>&times;</span>
                                </button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger">{error}</div>}
                                    <div className="row">
                                        <div className="form-group col-md-3">
                                            <label>Product Code</label>
                                            <input className="form-control" value={formData.productCode} onChange={(e) => setFormData({ ...formData, productCode: e.target.value })} required />
                                        </div>
                                        <div className="form-group col-md-5">
                                            <label>Name</label>
                                            <input className="form-control" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value, slug: formData.slug || toSlug(e.target.value) })} required />
                                        </div>
                                        <div className="form-group col-md-4">
                                            <label>Slug</label>
                                            <input className="form-control" value={formData.slug} onChange={(e) => setFormData({ ...formData, slug: e.target.value })} required />
                                        </div>
                                    </div>

                                    <div className="row">
                                        <div className="form-group col-md-3">
                                            <label>Type</label>
                                            <select className="form-control" value={formData.productType} onChange={(e) => setFormData({ ...formData, productType: e.target.value })}>
                                                <option value="Motorcycle">Motorcycle</option>
                                                <option value="Accessory">Accessory</option>
                                            </select>
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Category</label>
                                            <select className="form-control" value={formData.categoryId} onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })} required>
                                                <option value="">Select category</option>
                                                {filters.categories.map(cat => <option key={cat.id} value={cat.id}>{cat.name}</option>)}
                                            </select>
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Base Price</label>
                                            <input type="number" className="form-control" value={formData.basePrice} onChange={(e) => setFormData({ ...formData, basePrice: e.target.value })} min="0" required />
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Sale Price</label>
                                            <input type="number" className="form-control" value={formData.salePrice} onChange={(e) => setFormData({ ...formData, salePrice: e.target.value })} min="0" />
                                        </div>
                                    </div>

                                    <div className="row">
                                        <div className="form-group col-md-3">
                                            <label>Stock</label>
                                            <input type="number" className="form-control" value={formData.stockQuantity} onChange={(e) => setFormData({ ...formData, stockQuantity: e.target.value })} min="0" required />
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Status</label>
                                            <select className="form-control" value={formData.status} onChange={(e) => setFormData({ ...formData, status: e.target.value })}>
                                                <option value="Available">Available</option>
                                                <option value="Reserved">Reserved</option>
                                                <option value="Sold">Sold</option>
                                                <option value="Hidden">Hidden</option>
                                            </select>
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Showroom</label>
                                            <select className="form-control" value={formData.showroomId} onChange={(e) => setFormData({ ...formData, showroomId: e.target.value })}>
                                                <option value="">No showroom</option>
                                                {filters.showrooms.map(showroom => <option key={showroom.id} value={showroom.id}>{showroom.name}</option>)}
                                            </select>
                                        </div>
                                        <div className="form-group col-md-3">
                                            <label>Main Image URL</label>
                                            <input className="form-control" value={formData.mainImageUrl || ''} onChange={(e) => setFormData({ ...formData, mainImageUrl: e.target.value })} />
                                        </div>
                                    </div>

                                    {formData.productType === 'Motorcycle' && (
                                        <div className="row">
                                            <div className="form-group col-md-6">
                                                <label>Brand</label>
                                                <select className="form-control" value={formData.brandId} onChange={(e) => setFormData({ ...formData, brandId: e.target.value, carModelId: '' })}>
                                                    <option value="">Select brand</option>
                                                    {filters.brands.map(brand => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
                                                </select>
                                            </div>
                                            <div className="form-group col-md-6">
                                                <label>Model</label>
                                                <select className="form-control" value={formData.carModelId} onChange={(e) => setFormData({ ...formData, carModelId: e.target.value })}>
                                                    <option value="">Select model</option>
                                                    {carModelsByBrand.map(model => <option key={model.id} value={model.id}>{model.name}</option>)}
                                                </select>
                                            </div>
                                        </div>
                                    )}

                                    <div className="form-group">
                                        <label>Short Description</label>
                                        <textarea className="form-control" rows="2" value={formData.shortDescription || ''} onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })} />
                                    </div>
                                    <div className="form-group">
                                        <label>Description</label>
                                        <textarea className="form-control" rows="4" value={formData.description || ''} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary" onClick={closeModal}>Cancel</button>
                                    <button type="submit" className="btn btn-primary">{editingProduct ? 'Update' : 'Create'}</button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
            {showModal && <div className="modal-backdrop fade show"></div>}
        </div>
    );
};

export default Products;
