# RE4-SCENARIO-SMD-TOOLS
Extract and repack RE4 OG UHD/PS4/NS/X360/PS3/GC/WII Scenario SMD files

**Translate from Portuguese Brazil**

Programa destinado a extrair e recompactar os cenários usando somente um arquivo .OBJ;
<br>Nota: Esse repositório contém 4 executáveis, cada um serve para um conjunto de versões:
<br>-> RE4_UHD_SCENARIO_SMD_TOOL.exe : UHD (PC/STEAM)
<br>-> RE4_PS4NS_SCENARIO_SMD_TOOL.exe : PS4 e NS
<br>-> RE4_X360PS3_SCENARIO_SMD_TOOL.exe : X360 e PS3
<br>-> RE4_GCWII_SCENARIO_SMD_TOOL.exe : GC e WII

## Tutorial
Veja abaixo tutoriais em português de como usar a tool:
<br>[RE4 UHD Tutorial Editando Scenarios SMD](https://jaderlink.blogspot.com/2023/11/RE4-UHD-TUTORIAL-SCENARIO-SMD.html)
<br>[RE4 UHD Tutorial Editando r100.SMD](https://jaderlink.blogspot.com/2023/11/RE4-UHD-TUTORIAL-R100-SMD.html)

## Updates

**Update: V.1.3.0**
<br> * Foi reformulada a tool nessa versão do programa.
<br> * Adicionado um exe para suportar as versões de GC/WII;
<br> * Na versão de GC/WII os BIN são centralizados, o que faz com que os BINs repetidos fiquem na posição errada, então você vai ter que recalcular a posição manualmente.
<br> * Adicionado suporte para arquivos '.SHD';
<br> * Foi substituído o arquivo 'idxuhdscenario' pelo arquivo 'idxuuscenario'
<br> * E o arquivo 'idxuhdsmd' pelo arquivo 'idxuusmd', para ter um conteúdo mais simples de editar.
<br> * O conteúdo dentro do OBJ continua o mesmo, então você pode usar os OBJ gerados com a versão anterior do programa.
<br> * Feito melhorias na tool, e outras novidades. 

## RE4_\*\*_SCENARIO_SMD_TOOL.exe

Programa destinado a extrair e reempacotar os arquivos de cenário SMD do RE4 OG UHD/PS4/NS/X360/PS3/GC/WII;

## Extract:

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL_Extract_all_scenario_SMD.bat"
<br>Nesse exemplo, vou usar o arquivo: r204_004.SMD
<br>Ao extrair, serão gerados os arquivos:
<br>* "r204_004.scenario.idxuuscenario" ou "r204_004.scenario.idxggscenario" // arquivo importante de configurações, para o repack usando o .obj;
<br>* "r204_004.scenario.idxuusmd" ou "r204_004.scenario.idxggsmd" // arquivo importante de configurações, para o repack usando os arquivos '.BIN';
<br>* "r204_004.scenario.obj" // Conteúdo de todo o cenário, esse é o arquivo que você vai editar;
<br>* "r204_004.scenario.mtl" // Arquivo que acompanha o .obj;
<br>* "r204_004.scenario.idxmaterial" // conteúdo dos materiais (alternativa ao uso do .mtl);
<br>* "r204_004.scenario.idxuhdtpl" // representa o arquivo .tpl, presente no '.smd' exceto na versão GC/WII;
<br>* "r204_004.TPL" // Conteúdo do TPL presente no SMD, somente na versão GC/WII;
<br>* "r204_004_BIN\" //pasta contendo os arquivos ".BIN" (e os '.TPL', exceto na versão GC/WII);

## Repack:

Existem duas maneiras de fazer o repack.
<br>* Usando o arquivo '.idxuuscenario' ou '.idxggscenario', o repack será feito usando o arquivo '.obj';
<br>* Usando o arquivo '.idxuusmd' ou '.idxggsmd', o repack será feito com os arquivos '.bin' da pasta "r204_004_BIN";
<br>Nota: Os arquivos '.idxggscenario' e '.idxggsmd' são específicos da versão de GC/WII;
<br>E os arquivos '.idxuuscenario' e '.idxuusmd' são para as outras versões;


## Repack com '.idxuuscenario' para UHD/PS4/NS/X360/PS3

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL_Repack_all_with_idxuuscenario.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxuuscenario";
<br> que vai requisitar os arquivos:
<br>* r204_004.scenario.obj (obrigatório);
<br>* r204_004.scenario.mtl OU r204_004.scenario.idxmaterial + r204_004.scenario.idxuhdtpl;

Ao fazer o repack, serão gerados os arquivos:
<br>* "r204_004.SMD" (esse é o arquivo para ser colocado no .udas);
<br>* "r204_004.scenario.Repack.idxmaterial";
<br>* "r204_004.scenario.Repack.idxuhdtpl";
<br>* "r204_004.scenario.Repack.idxuusmd";
<br>* "r204_004_REPACK\" //pasta contendo os novos arquivos '.BIN' e o novo '.TPL'; (aviso: ele sobrescreve os arquivos);


## Repack com '.idxggscenario' somente para GC/WII

Use o bat: "RE4_GCWII_SCENARIO_SMD_TOOL_Repack_all_with_idxggscenario.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxggscenario";
<br> Que vai requisitar os arquivos:
<br>* r204_004.scenario.obj (obrigatório);
<br>* r204_004.TPL (obrigatório)
<br>* r204_004.scenario.mtl OU r204_004.scenario.idxmaterial;

Ao fazer o repack, serão gerados os arquivos:
<br>* "r204_004.SMD" (esse é o arquivo para ser colocado no .das);
<br>* "r204_004.scenario.Repack.idxggsmd";
<br>* "r204_004_REPACK\" //pasta contendo os novos arquivos '.BIN'; (aviso: ele sobrescreve os arquivos);


## Repack com '.idxuusmd' para UHD/PS4/NS/X360/PS3

Use o bat: "RE4_\*\*_SCENARIO_SMD_TOOL_Repack_all_with_idxuusmd.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxuusmd";
<br> Que vai requisitar os arquivos:
<br>* Os arquivos '.BIN' e '.TPL' da pasta "r204_004_BIN";

Ao fazer o repack, será gerado o arquivo:
<br>* "r204_004.SMD" (esse é o arquivo para ser colocado no .udas);

Nota: esse é o método antigo, no qual se edita os bin individualmente, porém o repack com .idxuuscenario cria novos bin modificados, e um novo .idxuusmd, no qual pode ser usado para fazer esse repack; essa opção é para caso você queira colocar um .bin no .smd que o programa não consiga criar.

## Repack com '.idxggsmd' somente para GC/WII

Use o bat: "RE4_GCWII_SCENARIO_SMD_TOOL_Repack_all_with_idxuusmd.bat";
<br>Nesse exemplo, vou usar o arquivo: "r204_004.scenario.idxggsmd";
<br> Que vai requisitar os arquivos:
<br>* r204_004.TPL (obrigatório)
<br>* Os arquivos '.BIN' da pasta "r204_004_BIN";

Ao fazer o repack, será gerado o arquivo:
<br>* "r204_004.SMD" (esse é o arquivo para ser colocado no .das);

## Sobre r204_004.scenario.obj

Esse arquivo é onde está todo o cenário, nele os arquivos BIN são separados por grupos, no qual a nomenclatura deve ser respeitada:
<br>
<br> *Exemplo:*
<br> **UHDSCENARIO#SMD_000#SMX_000#TYPE_08#BIN_000#**
<br> **UHDSCENARIO#SMD_001#SMX_001#TYPE_08#BIN_001#**
<br>
<br> *OU:*
<br> **UHDSCENARIO\_SMD\_000\_SMX\_000\_TYPE\_08\_BIN\_000\_**
<br> **UHDSCENARIO\_SMD\_001\_SMX\_001\_TYPE\_08\_BIN\_001\_**
<br>
<br>Aviso: no lugar de 'UHDSCENARIO' também pode ser:
<br>* UUSCENARIO
<br>* GGSCENARIO
<br>* MAINSCENARIO

Sendo:
<br>* É obrigatório o nome do grupo começar com "UHDSCENARIO" ou "UUSCENARIO" ou "GGSCENARIO" ou "MAINSCENARIO", e ser dividido por # ou _
<br>* A ordem dos campos não pode ser mudada;
<br>* SMD_000 -> esse é o ID da posição da Entry/Line no '.SMD', a numeração é em decimal;
<br>* SMX_000 -> esse é o ID do SMX, veja o arquivo '.SMX',  a numeração é em decimal;
<br>* TYPE_08 -> esse é um valor em hexadecimal que representa flags, veja mais abaixo sobre.
<br>* BIN_000 -> esse é o id do bin que será usado, para bin repetidos, será considerado somente o primeiro, (o próximo com o mesmo id, será usado o mesmo modelo que do primeiro).
<br>* o nome do grupo deve terminar com # ou _ (pois, após salvo o arquivo, o Blender coloca mais texto no final do nome do grupo);

----> Sobre verificações de grupos:
<br> * No Repack se ao lado direito do nome do grupo aparecer o texto "The group name is wrong;", significa que o nome do grupo está errado, e o seu arquivo SMD vai ficar errado;
<br> * E se ao lado direito aparecer "Warning: Group not used;" esse grupo está sendo ignorado pelo meu programa. Caso, na verdade, você gostaria de usá-lo, você deve arrumar o nome do grupo;


**Editando o arquivo .obj no Blender**
<br>No importador de .obj marque a caixa "Split By Group" que está no lado direito da tela.
<br>Com o arquivo importado, cada objeto representa um arquivo .BIN
<br>![Groups](Groups.png)

**Ao salvar o arquivo**
<br>Marque as caixas "Triangulated Mesh" e "Object Groups" e "Colors".
<br>No arquivo .obj o nome dos grupos vai ficar com "_Mesh" no final do nome (por isso, no editor, termina o nome do grupo com # para evitar problemas);

## Sobre os arquivos que começam com IDX
Segue abaixo a lista de comandos mais importantes presente no arquivo:

**Configurações Gerais:**
<br> * Magic:0040 // O valor do magic é oculto por padrão, e seu valor padrão é 0040, somente alguns valores são permitidos;
<br> * ExtraParameter_?:0 // caso o Magic for 0140, vão existir esses campos, que representam a quantidade de SMD Entry em cada um dos SMDs que estão dentro dos DAT (isso só existe no R100 onde usa o sistema de BLK)
<br> * SmdFileName:r204_004.SMD // esse é o nome do arquivo SMD que será gerado;
<br> * TplFileName:r204_004.TPL // esse é o nome do arquivo TPL que será colocado dentro do SMD; (somente na versão de GC/WII)
<br> * BinFolder:r204_004_BIN // esse é o nome da pasta onde serão salvos ou estão os arquivos .BIN (e o arquivo .TPL, exceto no GC/WII);
<br>* UseIdxUhdTpl:false // usa o conteúdo de UseIdxUhdTpl para forçar a ordem dos Ids dos TplEntry ao fazer o repack (campo somente no idxuuscenario);
<br>* UseIdxMaterial:false // Caso ativado, será o usado o arquivo '.idxmaterial' (e '.idxuhdtpl') ao invés do '.mtl' para fazer o repack (campo somente no idxuuscenario e idxggscenario);
<br>* EnableVertexColor:false // (Recomendo manter como false) Se ativado, cria os bins com o campo de "Vertex Color", mas o .obj não tem um suporte adequado para isso (campo somente no idxuuscenario e idxggscenario);
<br>* EnableDinamicVertexColor:true // o mesmo que o de cima, porém só vai criar o campo "Vertex Color" somente para os bins que realmente têm pintura de vértices. (campo somente no idxuuscenario e idxggscenario);

**Configuração Por SMD Entry**
<br> * SMD_000 // define um novo SMD entry, onde 000 é o ID do SMD, tudo o que vier abaixo dele vai ser referente a esse SMD entry, até que apareça outro SMD_001, podem estar em qualquer ordem, mas não repita a numeração.
<br> * PositionX:0.0 // posição X do bin na cena. 
<br> * PositionY:0.0 // posição Y do bin na cena. 
<br> * PositionZ:0.0 // posição Z do bin na cena. 
<br> * AngleX:0.0 // ângulo de rotação X do bin na cena.
<br> * AngleY:0.0 // ângulo de rotação Y do bin na cena.
<br> * AngleZ:0.0 // ângulo de rotação Z do bin na cena.
<br> * ScaleX:1.0 // escala X do bin na cena.
<br> * ScaleY:1.0 // escala Y do bin na cena.
<br> * ScaleZ:1.0 // escala Z do bin na cena.
<br> * TplFileID:0 // define o arquivo TPL associado ao arquivo bin, no arquivo 'idxggscenario' caso o valor desse campo seja zero, esse campo é omitido (no 'idxuuscenario' esse campo não é usado).
<br> // Por exemplo, se o nome do arquivo TPL for 'r204_004.TPL', para o TplFileID de ID 1 o nome do arquivo deve ser 'r204_004.1.TPL' e o de ID 2 deve ser 'r204_004.2.TPL', os valores aqui são em decimal.

**Os campos abaixo somente estão presentes no arquivo idxuusmd/idxggsmd**
<br> * BinFileID:0 // diz qual arquivo 'BIN' é usado (valor em decimal).
<br> * SmxID:0 // diz qual 'SMX' é usado e é vinculado ao arquivo 'SMX' (valor em decimal).
<br> * ObjectStatus:08 // valor em hexadecimal, é o mesmo que o campo "TYPE" no OBJ, veja mais abaixo sobre;
<br> // os campos abaixo são ocultos por padrão:
<br> * FixedFF:FF // sempre FF valor em hexadecimal
<br> * Unused1:0 // sempre 0 valor em hexadecimal
<br> * Unused2:0 // sempre 0 valor em hexadecimal
<br> * Unused3:0 // sempre 0 valor em hexadecimal
<br> * Unused4:0 // sempre 0 valor em hexadecimal
<br> * Unused5:0 // sempre 0 valor em hexadecimal
<br> * Unused6:0 // sempre 0 valor em hexadecimal
<br> * Unused7:0 // sempre 0 valor em hexadecimal

**Campos expecificos do idxuur100repack e idxggr100repack**
<br>* SharedFileName:r100_005.SMD // nome do SMD Shared
<br>* ExtraSmdFileName_?:r100_00_000.SMD // nome do SMD que faz parte do "SMD EXTRA"
<br>* ExtraTplFileName_?:R100.FILE_0.TPL // nome do TPL que vai dentro do "SMD EXTRA" (somente em 'idxggr100repack', opcional)

**Configuração Por SMD Entry para SMD_EXTRA**
<br>* FILE_00_SMD_000 // faz o mesmo que o "FILE_00_SMD_000" porem também define o ID do File Extra, é vinculado a entry do 'obj' que começa com FILE_00#SMD_000 

 ## sobre ObjectStatus / TYPE
Esse campo é um enum bitflag, isso significa que cada bit tem uma função, segue abaixo do que cada um faz:
 <br> * 0x00 / 0b00000000 // nenhuma flag ativada.
 <br> * 0x?1 / 0b?0001 // "EXE Scripted", contém evento associado ao modelo, não o remova do SMD
 <br> * 0x?2 / 0b?0010 // Desconhecido, não usado no jogo.
 <br> * 0x?4 / 0b?0100 // "Assign SMX Group ID"
 <br> * 0x?8 / 0b?1000 // "Ends SMX Group, Has SMX(?)"
 <br> * 0x1? / 0b0001? // "Use BIN from shared SMD (BLK)": Representa um BIN presente no SharedSMD;
 <br> * 0x2? / 0b0010? // "Use TPL from shared SMD (BLK)": não funciona no jogo.
 <br> * 0x4? / 0b0100? // "Use MOT from shared SMD (BLK)": MOT não funciona / não é usado.
 <br> * 0x8? / 0b1000? // Desconhecido, não usado no jogo.
 <br> // Outros valores são combinações dessas funcionalidades, veja bit a bit para saber o que faz.

# sobre .idxmaterial e .idxuhdtpl
Veja sobre em [RE4-UHD-BIN-TPL-TOOLS](https://github.com/JADERLINK/RE4-UHD-BIN-TPL-TOOLS);

# sobre '.idxr100extract' e '.idxuur100repack' ou 'idxggr100repack'
Para extrair o cenário, coloque os arquivos '.SMD' necessários ao lado de .idxr100extract;
<br> Nota: No tópico **tutorial** tem um tutorial sobre como editar o r100;

## Código de terceiro:

[ObjLoader by chrisjansson](https://github.com/chrisjansson/ObjLoader):
Encontra-se no RE4_SCENARIO_SMD_TOOLS\\CjClutter.ObjLoader.Loader, código modificado, as modificações podem ser vistas aqui: [link](https://github.com/JADERLINK/ObjLoader).

**At.te: JADERLINK**
<br>Thanks to \"mariokart64n\" and \"CodeMan02Fr\"
<br>Material information by \"Albert\"
<br>2025-10-25