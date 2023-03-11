let globalFile;

async function inputChanged() {
    globalFile = document.getElementById("inputFile").files[0];
    doSmth('adjust');
    drawFromString(await globalFile.text());
}

/**
 * @param {string} command
 */
async function doSmth(command) {
    if (!globalFile) {
        return;
    }
    let data = new FormData()
    data.append('file', globalFile)


    await fetch(`https://localhost:7220/api/${command}?`, {
        method: 'POST',
        body: data
      })
    .then(res => res.blob())
    .then(
        file => {
            globalFile = file;
        }
    )
    .catch(err => {
        console.log(err);
    });
    drawFromString(await globalFile.text());
}

async function doLine() {
    if (!globalFile) {
        return;
    }
    let data = new FormData()
    data.append('file', globalFile)

    let redDots;
    await fetch(`https://localhost:7220/api/line?`, {
        method: 'POST',
        body: data
      })
    .then(res => res.blob())
    .then(
        file => {
            redDots = file;
        }
    )
    .catch(err => {
        console.log(err);
    });
    drawFromString(await globalFile.text());
    drawFromStringLine(await redDots.text());
}


/**
 * @param {string} text
 */
function drawFromString(text) {
    let canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    let ctx = canvas.getContext('2d');
    let array = stringToArray(text);
    canvas.width = Math.max(...array.map(o => o[0])) + 10;
    canvas.height = Math.max(...array.map(o => o[1])) + 10;
    drawAllRecs(array,"black");
}
function drawFromStringLine(text) {
    let array = stringToArray(text);
    drawAllRecsLine(array,"red");
}
function drawAllRecsLine(array, color) {
    drawInfLine(array[array.length-2],array[array.length-1])
    for (let index = 0; index < array.length-2; index++) {
        drawRect(array[index][0],array[index][1], color);
    }
}
function drawInfLine(p1,p2) {
    let canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    let ctx = canvas.getContext('2d');
    let canvasHeight = ctx.canvas.width;
    for (let x = 0; x < canvasHeight; x++) {
        let y = (x-p1[0])*(p2[1]-p1[1])/(p2[0]-p1[0]) + p1[1];
        drawRect(x,y,"blue");
    }
}

function drawAllRecs(array, color) {
    for (let index = 0; index < array.length; index++) {
        drawRect(array[index][0],array[index][1], color);
    }
}

/**
 * @param {int} x
 * @param {int} y
 */
function drawRect(x, y, color) {
    let canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    let ctx = canvas.getContext('2d');

    ctx.beginPath();
    ctx.strokeStyle = color;
    ctx.lineWidth = 1;
    ctx.rect(x, y, 1, 1);
    ctx.stroke();
}

/**
 * @param {string} fileText 
 */
function stringToArray(fileText) {
    let array = fileText.match(/\d+\t\d+/g);
    for (let index = 0; index < array.length; index++) {
        array[index] = array[index].match(/\d+/g);
        array[index][0] = parseInt(array[index][0]); 
        array[index][1] = parseInt(array[index][1]);
    }
    return array;
}

function stringToArrayRed(fileText) {
    let array = fileText.match(/\d+red\d+/g);
    for (let index = 0; index < array.length; index++) {
        array[index] = array[index].match(/\d+/g);
        array[index][0] = parseInt(array[index][0]); 
        array[index][1] = parseInt(array[index][1]);
    }
    return array;
}


function downloadBlob(blob = globalFile, name = 'file.txt') {
    if (
      window.navigator && 
      window.navigator.msSaveOrOpenBlob
    ) return window.navigator.msSaveOrOpenBlob(blob);

    // For other browsers:
    // Create a link pointing to the ObjectURL containing the blob.
    const data = window.URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = data;
    link.download = name;

    // this is necessary as link.click() does not work on the latest firefox
    link.dispatchEvent(
      new MouseEvent('click', { 
        bubbles: true, 
        cancelable: true, 
        view: window 
      })
    );

    setTimeout(() => {
      // For Firefox it is necessary to delay revoking the ObjectURL
      window.URL.revokeObjectURL(data);
      link.remove();
    }, 100);
}