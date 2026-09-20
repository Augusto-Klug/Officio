// Mensagem transitória entre cadastro e listagem; não persiste dados do formulário.
let cadastroConfirmado = false
export function confirmarCadastro() { cadastroConfirmado = true }
export function consumirConfirmacaoCadastro(): boolean {
  const confirmado = cadastroConfirmado
  cadastroConfirmado = false
  return confirmado
}
