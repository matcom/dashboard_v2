import React, { useState } from 'react';
import {
  Modal, ModalHeader, ModalBody, ModalFooter,
  Button, Badge,
} from 'reactstrap';
import SelectionProfileCard from './SelectionProfileCard';

/**
 * Modal de revisión de autores externos.
 *
 * Recibe una lista de items provenientes de la resolución backend:
 *   [ { externalName: "García, Juan", match: { id, name, lastName, firstName, linkedUser? } | null } ]
 *
 * Para cada item:
 *  - Si hay coincidencia (match != null): muestra el nombre externo + la tarjeta del autor en
 *    el sistema + (si está vinculado) la tarjeta del usuario. El usuario puede aceptar la
 *    coincidencia o rechazarla (el nombre se tratará como nuevo autor externo).
 *  - Si no hay coincidencia (match == null): solo muestra el nombre externo; se añadirá como
 *    autor nuevo automáticamente.
 *
 * Al confirmar, llama a `onConfirm(coauthorTags)` con la lista de entradas listas para el
 * CoauthorPicker:
 *   - Aceptadas: { id, name, type: 'author', linkedUser? }
 *   - Rechazadas / sin coincidencia: { id: null, name: externalName, type: 'new' }
 */
export default function AuthorResolutionModal({ isOpen, items, onConfirm, onCancel }) {
  // decisions[externalName] = 'accept' | 'reject'
  const [decisions, setDecisions] = useState(() => {
    const init = {};
    (items ?? []).forEach(item => {
      init[item.externalName] = item.match ? 'accept' : 'none';
    });
    return init;
  });

  function toggle(externalName, value) {
    setDecisions(prev => ({ ...prev, [externalName]: value }));
  }

  function handleConfirm() {
    const tags = (items ?? []).map(item => {
      const decision = decisions[item.externalName];
      if (item.match && decision === 'accept') {
        return {
          id: item.match.id,
          name: item.match.name,
          type: 'author',
          linkedUser: item.match.linkedUser ?? null,
        };
      }
      // rejected or no match → new author entry
      return { id: null, name: item.externalName, type: 'new', linkedUser: null };
    });
    onConfirm(tags);
  }

  const matchCount = (items ?? []).filter(i => i.match).length;

  return (
    <Modal isOpen={isOpen} toggle={onCancel} size="lg" scrollable>
      <ModalHeader toggle={onCancel}>
        Revisión de autores externos
      </ModalHeader>

      <ModalBody>
        <p className="text-muted mb-3" style={{ fontSize: '0.9rem' }}>
          Se encontraron <strong>{matchCount}</strong> coincidencia{matchCount !== 1 ? 's' : ''} en el
          sistema para los autores importados. Revisa cada una y decide si la coincidencia detectada
          corresponde realmente al mismo autor o no.
        </p>

        <div className="d-flex flex-column gap-4">
          {(items ?? []).map(item => (
            <AuthorResolutionRow
              key={item.externalName}
              item={item}
              decision={decisions[item.externalName] ?? 'none'}
              onToggle={(val) => toggle(item.externalName, val)}
            />
          ))}
        </div>
      </ModalBody>

      <ModalFooter>
        <Button color="secondary" outline onClick={onCancel}>Cancelar</Button>
        <Button color="primary" onClick={handleConfirm}>
          Confirmar y añadir autores
        </Button>
      </ModalFooter>
    </Modal>
  );
}

// ── Row component ────────────────────────────────────────────────────────────

function AuthorResolutionRow({ item, decision, onToggle }) {
  const hasMatch = Boolean(item.match);

  return (
    <div
      style={{
        border: '1px solid #dee2e6',
        borderRadius: 8,
        padding: '1rem',
        backgroundColor: hasMatch ? '#f8f9fa' : '#fff',
      }}
    >
      {/* External name label */}
      <div className="d-flex align-items-center gap-2 mb-2">
        <Badge color="secondary" pill style={{ fontSize: '0.75rem' }}>Importado</Badge>
        <span style={{ fontFamily: 'monospace', fontWeight: 600, fontSize: '0.95rem' }}>
          {item.externalName}
        </span>
      </div>

      {!hasMatch && (
        <p className="text-muted mb-0" style={{ fontSize: '0.85rem' }}>
          No se encontró ningún autor con este nombre en el sistema.
          Se registrará como autor nuevo.
        </p>
      )}

      {hasMatch && (
        <>
          <p className="mb-2" style={{ fontSize: '0.85rem' }}>
            <i className="bi bi-search me-1 text-primary" />
            Posible coincidencia encontrada en el sistema:
          </p>

          <div
            style={{
              display: 'grid',
              gridTemplateColumns: item.match.linkedUser
                ? 'repeat(auto-fill, minmax(220px, 1fr))'
                : '1fr',
              gap: '0.75rem',
              maxWidth: item.match.linkedUser ? '100%' : 340,
              marginBottom: '0.75rem',
            }}
          >
            {/* Author card (without linked user) */}
            <div>
              <p className="text-muted mb-1" style={{ fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                Autor en el sistema
              </p>
              <AuthorCard match={item.match} />
            </div>

            {/* Linked user card */}
            {item.match.linkedUser && (
              <div>
                <p className="text-muted mb-1" style={{ fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Usuario vinculado
                </p>
                <SelectionProfileCard
                  person={{ ...item.match.linkedUser, type: 'user', linkedUser: null }}
                  variant="neutral"
                  onClick={null}
                />
              </div>
            )}
          </div>

          {/* Accept / Reject buttons */}
          <div className="d-flex gap-2 flex-wrap">
            <Button
              size="sm"
              color={decision === 'accept' ? 'success' : 'outline-success'}
              onClick={() => onToggle('accept')}
            >
              <i className="bi bi-check-circle me-1" />
              {decision === 'accept' ? 'Coincidencia aceptada' : 'Aceptar coincidencia'}
            </Button>
            <Button
              size="sm"
              color={decision === 'reject' ? 'danger' : 'outline-danger'}
              onClick={() => onToggle('reject')}
            >
              <i className="bi bi-x-circle me-1" />
              {decision === 'reject' ? 'Rechazada — se añade como nuevo' : 'Rechazar (añadir como nuevo)'}
            </Button>
          </div>
        </>
      )}
    </div>
  );
}

// ── Simple author card (no SelectionProfileCard dependency needed) ─────────────

function AuthorCard({ match }) {
  return (
    <div
      style={{
        border: '1px solid #0d6efd',
        borderRadius: 6,
        padding: '0.6rem 0.8rem',
        backgroundColor: '#e8f0fe',
        fontSize: '0.9rem',
      }}
    >
      <div style={{ fontWeight: 600 }}>{match.lastName}</div>
      {match.firstName && (
        <div className="text-muted" style={{ fontSize: '0.82rem' }}>{match.firstName}</div>
      )}
      {match.linkedUser && (
        <Badge color="success" className="mt-1" style={{ fontSize: '0.7rem' }}>
          <i className="bi bi-person-check me-1" />
          Usuario registrado
        </Badge>
      )}
    </div>
  );
}
