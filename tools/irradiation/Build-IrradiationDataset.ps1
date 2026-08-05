<#
.SYNOPSIS
    Recalcula a media estadual de irradiacao global horizontal a partir da API NASA POWER
    e emite o SQL de atualizacao da tabela irradiation_by_uf.

.DESCRIPTION
    Metodo: para cada UF, consulta N municipios geograficamente distribuidos (capital +
    interior) e tira a media aritmetica da climatologia anual (ALLSKY_SFC_SW_DWN, ANN).
    Usar varios pontos evita o vies de pegar so a capital — que no Brasil costuma ser
    litoranea e mais nublada que o interior do proprio estado (BA, MA e AP sao os casos
    mais gritantes).

    Fonte: NASA POWER (Prediction of Worldwide Energy Resources), CC BY 4.0.
    Diferente do Atlas do LABREN/INPE (CC BY-NC-ND), permite uso comercial e trabalho
    derivado — que e exatamente o que este script faz ao agregar pontos por UF.

.NOTES
    Rode este script para regerar o dataset; ele imprime o SQL pronto para virar migration.
    Nao roda em CI: e um utilitario manual, executado quando se quer atualizar a base.
#>

$ErrorActionPreference = 'Stop'

# Capital + municipios do interior, espalhados para cobrir a area do estado.
$pontosPorUf = [ordered]@{
  AC = @(@(-9.9754,-67.8249), @(-7.6281,-72.6753), @(-9.0658,-68.6573), @(-8.1653,-70.3543))
  AL = @(@(-9.6658,-35.7353), @(-9.7524,-36.6611), @(-9.3883,-37.9956), @(-9.4064,-36.6281))
  AP = @(@(0.0349,-51.0694),  @(3.8433,-51.8353),  @(-0.8028,-52.5142), @(0.7125,-51.4083))
  AM = @(@(-3.1190,-60.0217), @(-3.3544,-64.7111), @(-4.2525,-69.9381), @(-2.6283,-56.7358))
  BA = @(@(-12.9777,-38.5016),@(-12.1528,-44.9900),@(-14.8661,-40.8394),@(-9.4161,-40.5033), @(-14.7889,-39.0392))
  CE = @(@(-3.7319,-38.5267), @(-7.2131,-39.3153), @(-3.6892,-40.3500), @(-6.3592,-39.2986))
  DF = @(@(-15.7975,-47.8919),@(-15.6000,-47.7000))
  ES = @(@(-20.3155,-40.3128),@(-20.8489,-41.1128),@(-19.3911,-40.0722),@(-18.7161,-39.8589))
  GO = @(@(-16.6869,-49.2648),@(-17.7981,-50.9300),@(-15.5372,-47.3342),@(-13.4411,-49.1489))
  MA = @(@(-2.5307,-44.3068), @(-5.5264,-47.4822), @(-7.5325,-46.0356), @(-4.8589,-43.3556))
  MT = @(@(-15.6014,-56.0979),@(-11.8650,-55.5019),@(-16.4708,-54.6356),@(-15.8900,-52.2569))
  MS = @(@(-20.4697,-54.6201),@(-22.2214,-54.8056),@(-19.0089,-57.6528),@(-20.7511,-51.6783))
  MG = @(@(-19.9167,-43.9345),@(-18.9186,-48.2772),@(-16.7350,-43.8617),@(-21.7642,-43.3503),@(-15.4872,-44.3622))
  PA = @(@(-1.4558,-48.4902), @(-2.4400,-54.6989), @(-5.3689,-49.1178), @(-3.2033,-52.2103))
  PB = @(@(-7.1195,-34.8450), @(-7.2306,-35.8811), @(-7.0244,-37.2800), @(-6.8897,-38.5583))
  PR = @(@(-25.4284,-49.2733),@(-23.3103,-51.1628),@(-23.4253,-51.9386),@(-24.9556,-53.4553),@(-25.3953,-51.4581))
  PE = @(@(-8.0476,-34.8770), @(-9.3891,-40.5022), @(-8.2839,-35.9758), @(-7.9847,-38.2969))
  PI = @(@(-5.0892,-42.8019), @(-7.0767,-41.4672), @(-2.9047,-41.7767), @(-9.0747,-44.3589))
  RJ = @(@(-22.9068,-43.1729),@(-21.7544,-41.3300),@(-22.4692,-44.4467),@(-22.3711,-41.7869))
  RN = @(@(-5.7945,-35.2110), @(-5.1875,-37.3444), @(-6.4583,-37.0978), @(-6.2625,-36.5178))
  RS = @(@(-30.0346,-51.2177),@(-28.2628,-52.4067),@(-29.7547,-57.0883),@(-31.7714,-52.3428),@(-29.6842,-53.8069))
  RO = @(@(-8.7612,-63.9004), @(-10.8853,-61.9514),@(-12.7406,-60.1458),@(-10.7828,-65.3394))
  RR = @(@(2.8235,-60.6758),  @(1.8211,-61.1256),  @(-0.9403,-60.4394), @(4.4775,-61.1469))
  SC = @(@(-27.5954,-48.5480),@(-27.0964,-52.6183),@(-26.3044,-48.8456),@(-27.8156,-50.3258))
  SP = @(@(-23.5505,-46.6333),@(-22.9056,-47.0608),@(-21.1775,-47.8103),@(-22.1256,-51.3889),@(-20.8119,-49.3758))
  SE = @(@(-10.9472,-37.0731),@(-10.6853,-37.4247),@(-10.2189,-37.4211),@(-10.9167,-37.6700))
  TO = @(@(-10.2491,-48.3243),@(-7.1911,-48.2072), @(-11.7292,-49.0686),@(-10.7078,-48.4172))
}

$nomes = @{
  AC='Acre'; AL='Alagoas'; AP='Amapá'; AM='Amazonas'; BA='Bahia'; CE='Ceará'
  DF='Distrito Federal'; ES='Espírito Santo'; GO='Goiás'; MA='Maranhão'; MT='Mato Grosso'
  MS='Mato Grosso do Sul'; MG='Minas Gerais'; PA='Pará'; PB='Paraíba'; PR='Paraná'
  PE='Pernambuco'; PI='Piauí'; RJ='Rio de Janeiro'; RN='Rio Grande do Norte'
  RS='Rio Grande do Sul'; RO='Rondônia'; RR='Roraima'; SC='Santa Catarina'
  SP='São Paulo'; SE='Sergipe'; TO='Tocantins'
}

function Get-AnnualIrradiation($lat, $lon) {
    $url = "https://power.larc.nasa.gov/api/temporal/climatology/point" +
           "?parameters=ALLSKY_SFC_SW_DWN&community=RE&latitude=$lat&longitude=$lon&format=JSON"
    $resposta = Invoke-RestMethod -Uri $url -TimeoutSec 90
    return [double]$resposta.properties.parameter.ALLSKY_SFC_SW_DWN.ANN
}

$resultados = @()

foreach ($uf in $pontosPorUf.Keys) {
    $valores = @()
    foreach ($ponto in $pontosPorUf[$uf]) {
        try {
            $valores += Get-AnnualIrradiation $ponto[0] $ponto[1]
        } catch {
            Write-Warning "$uf ($($ponto[0]), $($ponto[1])): $($_.Exception.Message)"
        }
        Start-Sleep -Milliseconds 250
    }

    if ($valores.Count -eq 0) { Write-Warning "$uf sem nenhum ponto valido"; continue }

    $media = [math]::Round(($valores | Measure-Object -Average).Average, 2)
    $resultados += [PSCustomObject]@{
        UF     = $uf
        Estado = $nomes[$uf]
        Pontos = $valores.Count
        Min    = [math]::Round(($valores | Measure-Object -Minimum).Minimum, 2)
        Max    = [math]::Round(($valores | Measure-Object -Maximum).Maximum, 2)
        Media  = $media
    }
    Write-Host ("{0} {1,-20} {2} pontos  min {3}  max {4}  media {5}" -f `
        $uf, $nomes[$uf], $valores.Count, $resultados[-1].Min, $resultados[-1].Max, $media)
}

Write-Output ""
Write-Output "=============== TABELA ==============="
$resultados | Format-Table -AutoSize | Out-String -Width 200

Write-Output "=============== SQL ==============="
# Cultura invariante e obrigatoria aqui: em pt-BR o separador decimal vira virgula e
# quebraria o INSERT.
$invariante = [System.Globalization.CultureInfo]::InvariantCulture
foreach ($r in ($resultados | Sort-Object UF)) {
    $media = $r.Media.ToString('0.00', $invariante)
    Write-Output ("                    ('{0}', '{1}', {2}, 'NASA POWER (CERES/MERRA-2)', now())," -f `
        $r.UF, $r.Estado, $media)
}
