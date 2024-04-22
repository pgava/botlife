var botlife = (function() {
    var o = {};

    //---------------------------------------------------
    o.myPublicProperty = 0; // can be accessed by instantiated object's calling code

    var myPrivateProperty = 1; // can't be accessed outside of this module

    o.myPublicFunction = function() {
        myPrivateFunction();
        return myPrivateProperty;
    };

    function myPrivateFunction() {
        ++myPrivateProperty;
        ++o.myPublicProperty;
    }

    o.getMyPrivateProperty = function() {
        return myPrivateProperty;
    }
    //---------------------------------------------------

    const muBotType = 2;
    const psiBotType = 3;
    const etaBotType = 4;
    let timer;
    let arena;
    let bots = [];

    o.init = () => {
        paper.install(window);
    };

    o.start = () => {
        $.post("http://localhost:5202/start", function(data){
            console.log('start: ', data);
        }).done(() => {
            getNext();
        })
    };

    o.stop = () => {
        clearInterval(timer);
    }

    o.drawArena = () => {
        paper.setup('canvas');

        arena = new Path.Rectangle({
            center: [400, 300],
            size: [800, 600],
            fillColor: '#e9e9ff',
            strokeColor: 'black',
            strokeWidth: 2,
            rotation: 0
        });
    }

    let countNext = 0;
    const getNext = () => {
        console.log('getNext', countNext++);
        return $.get("http://localhost:5202/get-next", function(data){
            drawBots(data);
            // Schedule the next call to getNext after the current one is finished
            timer = setTimeout(getNext, 100);
        });
    };

    const drawBot = (data) => {
        var fillColor = 'red';
        if (data.type === psiBotType) {
            fillColor = 'green';
        }
        else if (data.type === etaBotType) {
            fillColor = 'blue';
        }

        const b = new Path.Rectangle({
            center: [data.position.x * 10 + 5, data.position.y * 10 + 5],
            size: [10, 10],
            fillColor: fillColor,
            strokeColor: 'black',
            strokeWidth: 0,
            rotation: 0,
            name: data.name
        });
        return b;
    }
    const drawBots = (data) => {
        if (Object.keys(bots).length === 0)
        {
            for (let i = 0; i < data.length; i++) {
                bots[data[i].name] = drawBot(data[i]);
            }
        }
        else {
            // Update the position of the bots
            for (let i = 0; i < data.length; i++) {
                let b = bots[data[i].name];
                if (b !== undefined)
                {
                    b.position = [data[i].position.x * 10 + 5, data[i].position.y * 10 + 5];
                }
                else {
                    bots[data[i].name] = drawBot(data[i]);
                }
            }

            // Remove any bots that are no longer in the data
            for (let b in bots) {
                let found = false;
                for (let i = 0; i < data.length; i++) {
                    if (data[i].name === b) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    bots[b].remove();
                    delete bots[b];
                }
            }
        }
    }

    return o;
})();
