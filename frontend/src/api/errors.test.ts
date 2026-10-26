import { afterEach, describe, expect, it } from 'vitest'
import { AxiosError, AxiosHeaders } from 'axios'
import { http } from './http'

afterEach(() => { delete http.defaults.adapter })

describe('structured API errors', () => {
  it('preserves the business code when the HTTP interceptor normalizes a conflict', async () => {
    http.defaults.adapter = async (config) => {
      throw new AxiosError('Conflict', 'ERR_BAD_REQUEST', config, undefined, {
        data: {
          code: 'ORCAMENTO_ACEITE_REQUER_CONFIRMAR_SUBSTITUICAO',
          error: 'Already accepted.',
        },
        status: 409,
        config,
        statusText: 'Conflict',
        headers: new AxiosHeaders(),
      })
    }

    await expect(http.post('/test')).rejects.toMatchObject({
      status: 409,
      code: 'ORCAMENTO_ACEITE_REQUER_CONFIRMAR_SUBSTITUICAO',
      message: 'Already accepted.',
    })
  })
})
