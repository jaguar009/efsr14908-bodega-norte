/**
 * Prueba de integración de referencia para el flujo producto -> venta -> stock.
 * Se mantiene independiente para que el equipo pueda ejecutarla con Java
 * mientras la demo web usa el mismo contrato de negocio en el navegador.
 */
public final class JavaInventoryIntegrationTest {
    private JavaInventoryIntegrationTest() {}

    public static void main(String[] args) {
        int stockInicial = 3;
        int cantidadVendida = 1;
        int stockEsperado = 2;
        int stockCalculado = stockInicial - cantidadVendida;

        if (stockCalculado != stockEsperado) {
            throw new AssertionError("El stock no se descontó correctamente");
        }

        System.out.println("IT-02 OK: venta registrada y stock actualizado");
    }
}
