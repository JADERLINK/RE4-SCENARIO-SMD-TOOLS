# Change Log

Aqui consta o conteúdo das alterações anteriores à V.1.3.0:

**Update: B.1.2.3**
<br>Adicionado suporte para as versões big endians PS3 e X360;
<br>Aviso: não misture os Bin/Tpl dessa versão com as das outras versões, pois vai dar erro no jogo e o programa não vai mais conseguir extrair o SMD;

**Update: B.1.2.2**
<br> Melhoria: melhorado a velocidade do repack, agora é muito rápido fazer o repack.
<br> Correção: corrigido o "width X height" no TPL que estava invertido nas versões anteriores. A ordem correta no arquivo é "height X width";
<br> E foram feitas melhorias gerais no código;

**Update: B.1.2.0**
<br>Adicionado suporte para as versões de PS4 e NS;
<br>Aviso: não misture os Bin/Tpl da versão UHD com a versão de PS4/NS, pois vai dar erro no jogo e o programa não vai mais conseguir extrair o SMD;

**Update: B.1.1.0**
<br>Adicionado o campo "EnableDinamicVertexColor" no arquivo ".idxuhdscenario", que ao ativar será colocado somente o conteúdo de "VertexColor" para os BINs que realmente tenham cor por vértice.
<br>Agora, numerações de BIN puladas serão preenchidas com um bin de 0 (zero) materiais.
<br>Nota: SMD_Entry pulados, ainda vai ser atribuído o BIN de ID 0;
<br>Agora, ao fazer repack com ".idxuhdscenario" o arquivo ".idxuhdtpl" será ignorado. Para usá-lo, você deve ativar a variável "UseIdxUhdTpl" dentro do ".idxuhdscenario";

**Update: B.1.0.09**
<br>Agora, ao extrair o arquivo .bin as "normals" serão normalizadas, em vez de ser dividida por um valor padrão, então agora é possível extrair os arquivos .bin gerados pela tool do percia sem erros.
<br> Ao fazer repack as normals do arquivo .obj serão normalizadas para evitar erros.
<br> O programa, ao gerar o arquivo .obj, não terá mais os zeros não significativos dos números, mudança feita para gerar arquivos menores.

**Update: B.1.0.0.8**
<br>Arrumado bug ao carregar o arquivo .idxmaterial;

**Update: B.1.0.0.7**
<br>Agora o programa é compatível em extrair e criar .SMD com arquivos .BIN acima do limite de vértices;
<br>Atenção: Os .BIN com vértices acima do limite só funcionam corretamente se eles forem usados dentro de arquivos Scenario .SMD;
<br>O uso acima do limite do vértice é permitido, mas use com cuidado.
<br>Em outras situações, o limite ainda é valido;

**Update: B.1.0.0.6**
<br>Corrigido bug no qual não era rotacionado as normals dos modelos que têm rotação,
então, caso esteja usando um .obj de versões anteriores, recalcule as normals;
<br>Corrigido um bug que, ao extrair as cores de vértices, estava sendo colocado de maneira errada no arquivo obj;

**Update: B.1.0.0.5**
<br>Corrigido bug no qual o arquivo MTL com PACK_ID com IDs que continham letras, as letras não eram consideradas.

**Update: B.1.0.0.4**
<br>Corrido erro, ao ter material sem a textura principal "map_Kd", será preenchido como Pack ID00000000 e texture ID 000;
<br> Agora, caso a quantidade de vértices for superior ao limite do arquivo, o programa vai avisar. (Não será criado o arquivo SMD);

**Update B.1.0.0.3**
<br>Corrigido bug que deformava a malha do modelo 3d, estava sendo criado faces do tipo "quad" de maneira errada; 

**Update B.1.0.0.2**
<br>Adicionado compatibilidade com outros editores 3D que não suportam caracteres especiais #: como, por exemplo, o 3dsMax;
<br> Adicionado também uma verificação no nome dos grupos, então caso esteja errado o nome, o programa avisa-rá;
<br> Os arquivos da versão anterior são compatíveis com essa versão;

**Update B.1.0.0.1**
<br> * Adicionado suporte para o **R100**, agora você pode extrair esse cenario dividido em 7 SMD, em um único arquivo .obj, use "R100.r100extract"; (Veja mais formações mais abaixo);
<br> * Adicionado verificação do "magic" do arquivo .Smd;
<br> * Nos arquivos ".idxuhdscenario" e ".idxuhdsmd" adicionados os campos "Magic" e "ExtraParameterAmount", no qual só vão aparecer caso forem usados.
<br> * O "Magic" padrão é o 0x0040;
<br> * Corrigido a extração do campo "vertexColors", no arquivo .obj;
<br> * Os arquivos da versão anterior são compatíveis com essa versão.