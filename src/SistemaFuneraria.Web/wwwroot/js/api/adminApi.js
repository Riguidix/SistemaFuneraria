/**
 * Módulo de API para administración de usuarios y roles (SF-029).
 * Consume los endpoints REST de /api/v1/admin/usuarios.
 */
export const adminApi = {
    /**
     * Obtiene la lista completa de usuarios operativos del sistema.
     * @param {string} apiBase - URL base del endpoint.
     * @returns {Promise<Array>} Lista de usuarios.
     */
    async listarUsuarios(apiBase = '/api/v1/admin/usuarios') {
        const response = await fetch(apiBase, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });
        if (!response.ok) {
            throw new Error(`Error al listar usuarios (${response.status})`);
        }
        return await response.json();
    },

    /**
     * Registra un nuevo usuario en el sistema.
     * @param {Object} data - Datos del usuario (NombreCompleto, Correo, Password, Rol, Sucursal).
     * @param {string} apiBase - URL base del endpoint.
     * @returns {Promise<Object>} Usuario creado.
     */
    async crearUsuario(data, apiBase = '/api/v1/admin/usuarios') {
        const response = await fetch(apiBase, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });
        if (!response.ok) {
            throw new Error(`Error al crear usuario (${response.status})`);
        }
        return await response.json();
    },

    /**
     * Actualiza información de perfil, rol o desbloquea una cuenta.
     * @param {string} id - Identificador de usuario.
     * @param {Object} data - Datos actualizados (NombreCompleto, Rol, Sucursal, Desbloquear).
     * @param {string} apiBase - URL base del endpoint.
     * @returns {Promise<Object>} Usuario actualizado.
     */
    async actualizarUsuario(id, data, apiBase = '/api/v1/admin/usuarios') {
        const response = await fetch(`${apiBase}/${encodeURIComponent(id)}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });
        if (!response.ok) {
            throw new Error(`Error al actualizar usuario (${response.status})`);
        }
        return await response.json();
    },

    /**
     * Elimina un usuario del sistema (prohíbe auto-eliminación).
     * @param {string} id - Identificador de usuario.
     * @param {string} apiBase - URL base del endpoint.
     * @returns {Promise<boolean>} True si fue exitoso.
     */
    async eliminarUsuario(id, apiBase = '/api/v1/admin/usuarios') {
        const response = await fetch(`${apiBase}/${encodeURIComponent(id)}`, {
            method: 'DELETE'
        });
        if (!response.ok) {
            throw new Error(`Error al eliminar usuario (${response.status})`);
        }
        return response.status === 204;
    }
};
