import React, { useState, useEffect } from 'react';
import { categoryApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const Categories = () => {
    const [categories, setCategories] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editingCategory, setEditingCategory] = useState(null);
    const [formData, setFormData] = useState({
        name: '',
        slug: '',
        parentCategoryId: '',
        description: '',
        sortOrder: 0,
        isActive: true,
    });
    const [error, setError] = useState('');
    const { isAdmin } = useAuth();

    useEffect(() => {
        loadCategories();
    }, []);

    const loadCategories = async () => {
        setLoading(true);
        try {
            const response = await categoryApi.getAll();
            setCategories(response.data || []);
        } catch (error) {
            console.error('Failed to load categories:', error);
        } finally {
            setLoading(false);
        }
    };

    const openModal = (category = null) => {
        if (category) {
            setEditingCategory(category);
            setFormData({
                name: category.name,
                slug: category.slug || '',
                parentCategoryId: category.parentCategoryId || '',
                description: category.description || '',
                sortOrder: category.sortOrder || 0,
                isActive: category.isActive ?? true,
            });
        } else {
            setEditingCategory(null);
            setFormData({
                name: '',
                slug: '',
                parentCategoryId: '',
                description: '',
                sortOrder: 0,
                isActive: true,
            });
        }
        setError('');
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingCategory(null);
        setError('');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const payload = {
                ...formData,
                parentCategoryId: formData.parentCategoryId ? parseInt(formData.parentCategoryId) : null,
                sortOrder: parseInt(formData.sortOrder) || 0,
            };

            if (editingCategory) {
                await categoryApi.update(editingCategory.id, {
                    id: editingCategory.id,
                    ...payload,
                });
            } else {
                await categoryApi.create(payload);
            }

            closeModal();
            loadCategories();
        } catch (error) {
            setError(error.response?.data?.message || 'Operation failed');
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Are you sure you want to delete this category?')) return;

        try {
            await categoryApi.delete(id);
            loadCategories();
        } catch (error) {
            alert('Failed to delete category. It may have associated products.');
        }
    };

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Catalog Categories</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">
                    <div className="card">
                        <div className="card-header">
                            <div className="row">
                                <div className="col-md-6">
                                    <h3 className="card-title">Cars, accessories and product groups</h3>
                                </div>
                                <div className="col-md-6 text-right">
                                    {isAdmin() && (
                                        <button className="btn btn-success" onClick={() => openModal()}>
                                            <i className="fas fa-plus"></i> Add Category
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
                                <table className="table table-bordered table-striped">
                                    <thead>
                                        <tr>
                                            <th style={{ width: '80px' }}>ID</th>
                                            <th>Name</th>
                                            <th>Slug</th>
                                            <th>Parent</th>
                                            <th>Description</th>
                                            <th>Status</th>
                                            {isAdmin() && <th style={{ width: '150px' }}>Actions</th>}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {categories.length === 0 ? (
                                            <tr>
                                                <td colSpan={isAdmin() ? 7 : 6} className="text-center">
                                                    No categories found
                                                </td>
                                            </tr>
                                        ) : (
                                            categories.map(category => (
                                                <tr key={category.id}>
                                                    <td>{category.id}</td>
                                                    <td>{category.name}</td>
                                                    <td>{category.slug}</td>
                                                    <td>{category.parentCategory?.name || '-'}</td>
                                                    <td>{category.description}</td>
                                                    <td>
                                                        <span className={`badge ${category.isActive ? 'badge-success' : 'badge-secondary'}`}>
                                                            {category.isActive ? 'Active' : 'Inactive'}
                                                        </span>
                                                    </td>
                                                    {isAdmin() && (
                                                        <td>
                                                            <button
                                                                className="btn btn-sm btn-info mr-1"
                                                                onClick={() => openModal(category)}
                                                            >
                                                                <i className="fas fa-edit"></i>
                                                            </button>
                                                            <button
                                                                className="btn btn-sm btn-danger"
                                                                onClick={() => handleDelete(category.id)}
                                                            >
                                                                <i className="fas fa-trash"></i>
                                                            </button>
                                                        </td>
                                                    )}
                                                </tr>
                                            ))
                                        )}
                                    </tbody>
                                </table>
                            )}
                        </div>
                    </div>
                </div>
            </section>

            {/* Modal */}
            {showModal && (
                <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1">
                    <div className="modal-dialog">
                        <div className="modal-content">
                            <div className="modal-header">
                                <h5 className="modal-title">
                                    {editingCategory ? 'Edit Category' : 'Add Category'}
                                </h5>
                                <button type="button" className="close" onClick={closeModal}>
                                    <span>&times;</span>
                                </button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger">{error}</div>}
                                    <div className="form-group">
                                        <label>Name</label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            value={formData.name}
                                            onChange={(e) => setFormData({
                                                ...formData,
                                                name: e.target.value,
                                                slug: formData.slug || e.target.value.trim().toLowerCase().replace(/\s+/g, '-')
                                            })}
                                            required
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Slug</label>
                                        <input
                                            type="text"
                                            className="form-control"
                                            value={formData.slug}
                                            onChange={(e) => setFormData({ ...formData, slug: e.target.value })}
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Parent Category</label>
                                        <select
                                            className="form-control"
                                            value={formData.parentCategoryId}
                                            onChange={(e) => setFormData({ ...formData, parentCategoryId: e.target.value })}
                                        >
                                            <option value="">Root category</option>
                                            {categories
                                                .filter(category => category.id !== editingCategory?.id)
                                                .map(category => (
                                                    <option key={category.id} value={category.id}>{category.name}</option>
                                                ))}
                                        </select>
                                    </div>
                                    <div className="form-group">
                                        <label>Description</label>
                                        <textarea
                                            className="form-control"
                                            value={formData.description}
                                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                            rows="3"
                                        />
                                    </div>
                                    <div className="form-group">
                                        <label>Sort Order</label>
                                        <input
                                            type="number"
                                            className="form-control"
                                            value={formData.sortOrder}
                                            onChange={(e) => setFormData({ ...formData, sortOrder: e.target.value })}
                                        />
                                    </div>
                                    <div className="form-check">
                                        <input
                                            type="checkbox"
                                            className="form-check-input"
                                            id="categoryActive"
                                            checked={formData.isActive}
                                            onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                        />
                                        <label className="form-check-label" htmlFor="categoryActive">Active</label>
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary" onClick={closeModal}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="btn btn-primary">
                                        {editingCategory ? 'Update' : 'Create'}
                                    </button>
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

export default Categories;
